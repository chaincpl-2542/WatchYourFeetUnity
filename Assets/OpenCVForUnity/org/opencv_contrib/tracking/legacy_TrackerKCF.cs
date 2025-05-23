
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenCVForUnity.TrackingModule
{

    // C++: class TrackerKCF
    /// <summary>
    ///  the KCF (Kernelized Correlation Filter) tracker
    /// </summary>
    /// <remarks>
    ///     KCF is a novel tracking framework that utilizes properties of circulant matrix to enhance the processing speed.
    ///     This tracking method is an implementation of @cite KCF_ECCV which is extended to KCF with color-names features (@cite KCF_CN).
    ///     The original paper of KCF is available at &lt;http://www.robots.ox.ac.uk/~joao/publications/henriques_tpami2015.pdf&gt;
    ///     as well as the matlab implementation. For more information about KCF with color-names features, please refer to
    ///     &lt;http://www.cvl.isy.liu.se/research/objrec/visualtracking/colvistrack/index.html&gt;.
    /// </remarks>
    public class legacy_TrackerKCF : legacy_Tracker
    {

        protected override void Dispose(bool disposing)
        {

            try
            {
                if (disposing)
                {
                }
                if (IsEnabledDispose)
                {
                    if (nativeObj != IntPtr.Zero)
                        tracking_legacy_1TrackerKCF_delete(nativeObj);
                    nativeObj = IntPtr.Zero;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }

        }

        protected internal legacy_TrackerKCF(IntPtr addr) : base(addr) { }

        // internal usage only
        public static new legacy_TrackerKCF __fromPtr__(IntPtr addr) { return new legacy_TrackerKCF(addr); }

        //
        // C++: static Ptr_legacy_TrackerKCF cv::legacy::TrackerKCF::create()
        //

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="parameters">
        /// KCF parameters TrackerKCF::Params
        /// </param>
        public static legacy_TrackerKCF create()
        {


            return legacy_TrackerKCF.__fromPtr__(DisposableObject.ThrowIfNullIntPtr(tracking_legacy_1TrackerKCF_create_10()));


        }


#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
#else
        const string LIBNAME = "opencvforunity";
#endif



        // C++: static Ptr_legacy_TrackerKCF cv::legacy::TrackerKCF::create()
        [DllImport(LIBNAME)]
        private static extern IntPtr tracking_legacy_1TrackerKCF_create_10();

        // native support for java finalize()
        [DllImport(LIBNAME)]
        private static extern void tracking_legacy_1TrackerKCF_delete(IntPtr nativeObj);

    }
}
