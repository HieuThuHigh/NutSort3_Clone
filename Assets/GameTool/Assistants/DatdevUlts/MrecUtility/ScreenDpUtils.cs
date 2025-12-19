using GameTool.APIs.Scripts;
using UnityEngine;

namespace GameTool.APIs.Scripts.Ads
{
    public class ScreenDpUtils
    {
        public static float bannerHeight = 50;
        public static float bannerWidth = 320;
        public static float mrecHeight = 250;
        public static float mrecWidth = 300;

        public static MediationType mediationTypeBanner = MediationType.Applovin;

        /// Tính toán vị trí (tọa độ X và Y) theo đơn vị dp (density-independent pixels) để hiển thị banner hình chữ nhật trung bình (MREC) ở phía dưới màn hình.
        /// Phép tính xét đến chiều cao của MREC, chiều cao vùng an toàn trên và dưới, cũng như chiều cao banner nếu có.
        /// <param name="haveBanner">
        /// Tham số boolean xác định có hiển thị banner khác (ví dụ: banner quảng cáo thích ứng) hay không.
        /// True nếu có banner hiển thị, ngược lại là False.
        /// </param>
        /// <returns>
        /// Một giá trị Vector2 đại diện cho tọa độ X và Y trong đơn vị dp để hiển thị banner MREC ở phía dưới màn hình.
        /// </returns>
        public static Vector2 GetPosDpShowMrecBottom(bool haveBanner = true)
        {
            var adaptiveBannerHeight = GetHeightBanner();
            var yMax = mrecHeight + adaptiveBannerHeight + GetHeightTopSafeInDp() + GetHeightBottomSafeInDp() + adaptiveBannerHeight/3;



            if (!haveBanner)
            {
                yMax = mrecHeight + GetHeightTopSafeInDp() + GetHeightBottomSafeInDp();
            }

            var x = GetXCenterDpMrec();
            var y = GetHeightInDp() - (GetHeightTopSafeInDp() + GetHeightBottomSafeInDp()) - yMax;
            
#if USE_APPLOVIN_ADS
            if (haveBanner)
            {
                var rectBanner = MaxSdk.GetBannerLayout(GameToolSettings.Instance.applovinBannerID);
                Debug.LogError($"rectBanner.height: {rectBanner.height}");
                Debug.LogError($"adaptiveBannerHeight: {adaptiveBannerHeight}");
                y = rectBanner.y - mrecHeight - (rectBanner.height / 2)-(rectBanner.height / 3);
            }
#endif
            
            return new Vector2(x, y);
        }

        public static void SetSafeAreaAnchorsWithMrec(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Chuyển đổi sang tỉ lệ (0–1) theo resolution
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            anchorMin.y = GetYTopInVpMrec();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetSafeAreaAnchorsWithBanner(RectTransform rectTransform, bool canshowBanner = true)
        {
            if (rectTransform == null)
                return;

            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Chuyển đổi sang tỉ lệ (0–1) theo resolution
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            if (canshowBanner)
            {
                anchorMin.y = GetYTopInVpBanner();
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetSizeMrec(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Chuyển đổi sang tỉ lệ (0–1) theo resolution
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            anchorMax.y = GetYTopInVpMrec();
            anchorMin.y = GetYMinInVpMrec();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// Tính toán tọa độ Y đã được chuẩn hóa trong viewport (trong khoảng từ 0 đến 1) cho ranh giới trên của banner hình chữ nhật trung bình (MREC).
        /// Phép tính này xét đến chiều cao của MREC, chiều cao của banner thích ứng nếu có, chiều cao phần trên của vùng an toàn (safe area),
        /// và chiều cao phần dưới của vùng an toàn.
        /// <returns>
        /// Một giá trị float đại diện cho tọa độ Y đã chuẩn hóa của vị trí trên cùng của MREC trong viewport.
        /// </returns>
        public static float GetYTopInVpMrec()
        {
            var adaptiveBannerHeight = GetHeightBanner();
            var yMaxDp = mrecHeight + adaptiveBannerHeight + GetHeightBottomSafeInDp() + adaptiveBannerHeight/3;
            return GetViewport01Y(yMaxDp);
        }
        public static float GetYMinInVpMrec()
        {
            var adaptiveBannerHeight = GetHeightBanner();
            var yMaxDp = adaptiveBannerHeight + GetHeightBottomSafeInDp() + adaptiveBannerHeight/3;
            return GetViewport01Y(yMaxDp);
        }
        
        public static float GetYTopInVpBanner()
        {
            var adaptiveBannerHeight = GetHeightBanner();
            var yMaxDp = adaptiveBannerHeight + GetHeightBottomSafeInDp();
            return GetViewport01Y(yMaxDp);
        }

        /// Tính toán tọa độ X theo đơn vị dp (density-independent pixels) để định vị banner hình chữ nhật trung bình (MREC) ở trung tâm màn hình theo chiều ngang.
        /// Vị trí được tính dựa trên chiều rộng màn hình và chiều rộng của MREC. 
        /// <returns>
        /// Một giá trị float đại diện cho tọa độ X trong đơn vị dp để hiển thị banner MREC ở trung tâm màn hình theo chiều ngang.
        /// </returns>
        public static float GetXCenterDpMrec()
        {
            return GetWidthInDp() / 2f - mrecWidth / 2f;
        }

        /// Tính toán và trả về chiều cao của banner quảng cáo dựa trên loại mediation đang sử dụng.
        /// Nếu loại mediation là Applovin, nó sẽ lấy chiều cao banner thích ứng từ MaxSdkUtils.
        /// Nếu không, sử dụng chiều cao mặc định giá trị được định nghĩa trong lớp.
        /// <returns>
        /// Một giá trị float đại diện cho chiều cao của banner quảng cáo trong đơn vị dp.
        /// </returns>
        private static float GetHeightBanner()
        {
            if (mediationTypeBanner == MediationType.Applovin)
            {
#if USE_APPLOVIN_ADS
                return MaxSdkUtils.GetAdaptiveBannerHeight();
#endif
            }

            return bannerHeight;
        }

        /// Tính toán chiều cao của màn hình thiết bị theo đơn vị dp (density-independent pixels).
        /// Phép tính sử dụng chiều cao của màn hình thiết bị (pixel) chia cho mật độ màn hình (screen density).
        /// <returns>
        /// Một giá trị kiểu float thể hiện chiều cao của màn hình thiết bị theo đơn vị dp.
        /// </returns>
        public static float GetHeightInDp()
        {
            return Screen.height / GetScreenDensity();
        }

        /// Tính toán vị trí Y trên màn hình theo đơn vị dp (density-independent pixels) từ giá trị viewport tỷ lệ chuẩn (giá trị từ 0 đến 1).
        /// Phép tính sử dụng chiều cao màn hình thiết bị theo đơn vị dp và giá trị viewport tỷ lệ chuẩn được cung cấp.
        /// <param name="vp">
        /// Giá trị viewport tỷ lệ chuẩn (float), là giá trị từ 0 đến 1, biểu thị vị trí tỷ lệ theo chiều dọc trên màn hình.
        /// </param>
        /// <returns>
        /// Một giá trị kiểu float thể hiện vị trí Y trên màn hình theo đơn vị dp.
        /// </returns>
        public static float GetViewportYInDp(float vp)
        {
            return GetHeightInDp() * vp;
        }

        /// Tính toán giá trị Y trong hệ quy chiếu viewport (giá trị từ 0 đến 1) dựa trên giá trị dp (density-independent pixels).
        /// Phép tính này sử dụng chiều cao màn hình thiết bị tính theo dp để chuẩn hóa giá trị Y.
        /// <param name="dp">
        /// Tham số kiểu float đại diện cho giá trị chiều cao hoặc tọa độ trong hệ dp (density-independent pixels).
        /// </param>
        /// <returns>
        /// Một giá trị kiểu float thể hiện tọa độ Y trong hệ quy chiếu viewport (từ 0 đến 1).
        /// </returns>
        public static float GetViewport01Y(float dp)
        {
            return dp / GetHeightInDp();
        }

        /// Tính toán giá trị tọa độ X trong hệ quy chiếu viewport 0-1 (viewport normalized coordinates) dựa trên giá trị dp (density-independent pixels).
        /// Phép tính sử dụng chiều rộng màn hình được chuyển đổi sang đơn vị dp để chuẩn hóa tọa độ.
        /// <param name="dp">
        /// Giá trị tọa độ X theo đơn vị dp cần chuyển đổi sang hệ viewport 0-1.
        /// </param>
        /// <returns>
        /// Giá trị tọa độ X trong hệ quy chiếu viewport 0-1, được tính bằng cách chuẩn hóa giá trị dp theo chiều rộng màn hình.
        /// </returns>
        public static float GetViewport01X(float dp)
        {
            return dp / GetWidthInDp();
        }

        /// Tính toán chiều cao vùng an toàn phía trên của màn hình theo đơn vị dp (density-independent pixels).
        /// Chiều cao vùng an toàn phía trên được xác định dựa trên sự khác biệt giữa chiều cao tổng màn hình và
        /// tọa độ y trên cùng của vùng an toàn (yMax của Screen.safeArea).
        /// <returns>
        /// Một giá trị kiểu float đại diện cho chiều cao vùng an toàn phía trên màn hình theo đơn vị dp.
        /// </returns>
        public static float GetHeightTopSafeInDp()
        {
            float topSafeHeightPx = Screen.height - Screen.safeArea.yMax;
            return topSafeHeightPx / GetScreenDensity();
        }

        /// Tính toán chiều cao vùng an toàn phía dưới màn hình theo đơn vị dp (density-independent pixels).
        /// Dữ liệu được tính dựa trên tọa độ của vùng an toàn phía dưới màn hình bằng pixel và mật độ màn hình (screen density).
        /// <returns>
        /// Chiều cao vùng an toàn phía dưới màn hình được biểu diễn dưới dạng một giá trị float trong đơn vị dp.
        /// </returns>
        public static float GetHeightBottomSafeInDp()
        {
            float bottomSafeHeightPx = Screen.safeArea.y;
            return bottomSafeHeightPx / GetScreenDensity();
        }

        /// Tính toán và trả về chiều rộng của màn hình theo đơn vị dp (density-independent pixels).
        /// Việc chuyển đổi sử dụng độ phân giải màn hình hiện tại và mật độ màn hình (screen density) để tính toán chính xác.
        /// <returns>
        /// Một giá trị float đại diện cho chiều rộng của màn hình theo đơn vị dp.
        /// </returns>
        public static float GetWidthInDp()
        {
            return Screen.width / GetScreenDensity();
        }

        /// Lấy giá trị mật độ màn hình (screen density) theo đơn vị dp (density-independent pixels).
        /// Giá trị này được tính dựa trên DPI (dots per inch) của thiết bị hoặc từ SDK của bên thứ ba tùy thuộc vào loại "MediationType" được cấu hình.
        /// Mặc định sẽ sử dụng Applovin nếu không chỉ định loại MediationType cụ thể.
        /// <param name="mediationType">
        /// Loại "MediationType" sử dụng để lấy giá trị mật độ màn hình (screen density).
        /// Các giá trị bao gồm: Admob, Applovin, IronSource hoặc None.
        /// Mặc định là Applovin.
        /// </param>
        /// <returns>
        /// Giá trị float đại diện cho mật độ màn hình (screen density).
        /// Nếu không thể lấy giá trị DPI, phương pháp sử dụng giá trị mặc định phù hợp với nền tảng:
        /// 160 cho Android, 163 cho iOS và 96 cho những nền tảng khác.
        /// </returns>
        public static float GetScreenDensity(MediationType mediationType = MediationType.Applovin)
        {
            if (API.IsEditor())
            {
                return 3.375f;
            }
            
            if (mediationType == MediationType.Applovin)
            {
#if USE_APPLOVIN_ADS
                return MaxSdkUtils.GetScreenDensity();
#endif
            }

            float dpi = Screen.dpi;

            if (dpi == 0)
            {
#if UNITY_ANDROID
                dpi = 160f;
#elif UNITY_IOS
        dpi = 163f;
#else
        dpi = 96f;
#endif
            }

            return dpi / 160f;
        }
    }
}