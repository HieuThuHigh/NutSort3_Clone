namespace GameToolSample.Scripts.Enum
{
    public static class AnalyticID
    {
        public enum ScreenID
        {
            none,
            home,
            gameplay,
            setting,
            pause,
            shop,
            skin_shop,
            spin,
            daily_reward,
            victory,
            lose,
            revive,
            new_version,
            LanguagePopup
        }

        public enum ButtonID
        {
            none,
            play,
            home,
            close,
            open,
            setting,
            sound,
            music,
            vibration,
            more_coin,
            more_diamond,
            ads_coin_iap,
            ads_diamond_iap,
            restore_iap,
            remove_ads,
            no_thanks,
            get,
            trying,
            select,
            pause,
            replay,
            resume,
            buy_free,
            buy_by_iap,
            buy_by_diamond,
            buy_by_coin,
            open_free,
            open_by_ads,
            open_by_coin,
            open_by_diamond,
            x2_reward_free,
            x2_reward_ads,
            get_it,
            continues,
            revive_free,
            revive_ads,
            rewards_free,
            rewards_ads,
            open_spin,
            spin_free,
            spin_ads,
            shop_iap,
            shop_skin,
            special,
            more_game,
            random_by_coin,
            random_by_diamond,
            yes,
            no,
            update
        }

        public enum MonetizationState
        {
            none,
            click,
            loaded,
            completed,
            pending,
            load_failed,
            show_failed,
            deny,
            show_success,
            close,
            failed,
            request,
            show_request,
            server_ready,
            interval_ready,
            server_failed,
            interval_failed
        }

        public enum AdsType
        {
            none,
            banner,
            interstitial,
            rewarded,
            iap,
            app_open
        }

        public enum GamePlayEvent
        {
            none,
            game_start,
            game_playing,
            game_end
        }

        public enum GamePlayParam
        {
            none,
            level,
            mode,
            map,
            location,
            state
        }

        public enum GamePlayState
        {
            none,
            victory,
            lose,
            die,
            revive,
            quit,
            skip,
            pause,
            playing,
            replay
        }

        public enum GameEconomyState
        {
            none,
            spend,
            earn
        }

        public enum GameItemState
        {
            none,
            unlock,
            trying,
            select,
            preview
        }

        public enum UseBehaviourState
        {
            none,
            app_quit,
            uninstall
        }

        public enum LocationTracking
        {
            none = 0,
            gameplay = 1,
            buygold = 2,
            buyads = 3,
        }

        public enum GameModeName
        {
            none
        }

        public enum KeyAds
        {
            none
        }

        // public class AdjustEventToken
        // {
        //     public static string first_open = "oyvccc";
        //     public static string inter_click = "n4fccc";
        //     public static string inter_impression = "h66ccc";
        //     public static string reward_click = "1zcccc";
        //     public static string reward_completed = "8ojccc";
        //     public static string reward_impression = "eopccc";
        // }
    }
}
