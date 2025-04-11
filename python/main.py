import cv2
import numpy as np
import mediapipe as mp
from ultralytics import YOLO
from deep_sort_realtime.deepsort_tracker import DeepSort

# โหลดโมเดล
yolo = YOLO("yolov8n.pt")  # ใช้ yolov8n เพื่อความเร็ว
tracker = DeepSort(max_age=10)
mp_selfie = mp.solutions.selfie_segmentation.SelfieSegmentation(model_selection=1)

# กล้อง
cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret: break
    orig = frame.copy()

    results = yolo(frame, classes=[0])  # class 0 = person
    detections = []
    for result in results:
        for box in result.boxes:
            x1, y1, x2, y2 = map(int, box.xyxy[0])
            conf = float(box.conf[0])
            detections.append(([x1, y1, x2 - x1, y2 - y1], conf, 'person'))

    tracks = tracker.update_tracks(detections, frame=frame)

    result_img = np.zeros_like(frame)

    for track in tracks:
        if not track.is_confirmed(): continue
        track_id = track.track_id
        l, t, r, b = map(int, track.to_ltrb())
        l, t = max(0, l), max(0, t)
        r, b = min(frame.shape[1], r), min(frame.shape[0], b)

        # กรอบและ ID ใน original
        cv2.rectangle(orig, (l, t), (r, b), (0, 255, 0), 2)
        cv2.putText(orig, f'ID: {track_id}', (l, t - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0,255,0), 2)

        # ตัดพื้นหลังเฉพาะคนนั้น
        person_crop = frame[t:b, l:r]
        if person_crop.size == 0: continue  # เผื่อหลุดขอบภาพ

        rgb = cv2.cvtColor(person_crop, cv2.COLOR_BGR2RGB)
        result = mp_selfie.process(rgb)
        mask = (result.segmentation_mask > 0.5).astype(np.uint8) * 255

        person_fg = cv2.bitwise_and(person_crop, person_crop, mask=mask)

        # แปะลง result_img ที่ตำแหน่งเดิม
        result_img[t:b, l:r] = person_fg

    # รวมภาพ ซ้าย = original / ขวา = ตัดพื้นหลัง
    combined = np.hstack((orig, result_img))
    cv2.imshow("Original + Background Removed", combined)

    if cv2.waitKey(1) == ord('q'): break

cap.release()
cv2.destroyAllWindows()
