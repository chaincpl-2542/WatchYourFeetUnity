import cv2
import numpy as np
from ultralytics import YOLO
from deep_sort_realtime.deepsort_tracker import DeepSort
import socket
import json

# === CONFIG ===
YOLO_MODEL = "yolov8n.pt"
UDP_IP = "127.0.0.1"
UDP_PORT = 5055

yolo = YOLO(YOLO_MODEL)
tracker = DeepSort(max_age=10)
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    h, w, _ = frame.shape
    results = yolo(frame, classes=[0])  # detect person only
    detections = []

    for result in results:
        for box in result.boxes:
            x1, y1, x2, y2 = map(int, box.xyxy[0])
            conf = float(box.conf[0])
            detections.append(([x1, y1, x2 - x1, y2 - y1], conf, 'person'))

    tracks = tracker.update_tracks(detections, frame=frame)

    for track in tracks:
        if not track.is_confirmed():
            continue

        track_id = track.track_id
        l, t, r, b = map(int, track.to_ltrb())
        l, t = max(0, l), max(0, t)
        r, b = min(w, r), min(h, b)
        cx = (l + r) // 2
        cy = (t + b) // 2

        data = {
            "id": int(track_id),
            "x": cx,
            "y": cy
        }

        message = json.dumps(data).encode('utf-8')
        sock.sendto(message, (UDP_IP, UDP_PORT))

    cv2.imshow("Tracking", frame)
    if cv2.waitKey(1) == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
