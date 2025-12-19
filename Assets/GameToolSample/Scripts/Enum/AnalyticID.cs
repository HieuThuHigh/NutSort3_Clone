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
            main_menu = 1,
            gameplay = 2,
            spin_popup = 3,
            daily_reward_popup = 4,
            skin_popup = 5,
            shop_popup = 6,
            coin_pack_shop = 7,
            diamond_pack_shop = 8,
            coin_ads_shop = 9,
            diamond_ads_shop = 10,
            lose_popup = 11,
            victory_popup = 12,
            x2_reward_victory = 13,
            revive_popup = 14,
            x2_reward_spin = 15,
            unlock_by_diamond_skin_popup = 16,
            unlock_by_coin_skin_popup = 17,
            random_skin_coin = 18,
            random_skin_diamond = 19,
            more_coin_skin_popup = 20,
            unlock_by_ads_skin_popup = 21,
            new_skin_popup = 22,
            more_time_popup = 23,
            booster_popup = 24,
            bonus_diamond_ads_shop = 25,
            x2_daily_reward = 26,
            bonus_x2_daily_reward = 27,
            bonus_more_time = 28,
            bonus_revive_popup = 29,
            bonus_try_skill = 30,
            skill_popup = 31,
            coin_ads_shop_skin = 32,
            bonus_skin_popup = 33,
            bonus_coin_ads_skin_popup = 34,
            bonus_skin_ads_skin_popup = 35,
            unlock_skin_ads_skin_popup = 36,
            bonus_x2_reward_victory = 37,
            close_victory_popup = 38,
            bonus_get_new_skin = 39,
            get_new_skin = 40,
            get_diamond_new_skin = 41,
            spin_ads = 42,
            bonus_spin_ads = 43,
            bonus_x2_reward_spin = 44,
            setting_replay = 45,
            setting_back_home = 46,
            splash = 47,
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
