﻿using System.Collections.Generic;

namespace Oblivion.HabboHotel.Users
{
    internal enum ChatEmotion
    {
        Smile,
        Angry,
        Sad,
        Shocked,
        None
    }

    internal static class ChatEmotions
    {
        private static Dictionary<string, ChatEmotion> _mEmotions;

        internal static void Initialize()
        {
            _mEmotions = new Dictionary<string, ChatEmotion>
            {
                // Smile
                {":)", ChatEmotion.Smile},
                {";)", ChatEmotion.Smile},
                {":d", ChatEmotion.Smile},
                {";d", ChatEmotion.Smile},
                {":]", ChatEmotion.Smile},
                {";]", ChatEmotion.Smile},
                {"=)", ChatEmotion.Smile},
                {"=]", ChatEmotion.Smile},
                {":-)", ChatEmotion.Smile},

                // Angry
                {">:(", ChatEmotion.Angry},
                {">:[", ChatEmotion.Angry},
                {">;[", ChatEmotion.Angry},
                {">;(", ChatEmotion.Angry},
                {">=(", ChatEmotion.Angry},
                {":@", ChatEmotion.Angry},

                // Shocked
                {":o", ChatEmotion.Shocked},
                {";o", ChatEmotion.Shocked},
                {">;o", ChatEmotion.Shocked},
                {">:o", ChatEmotion.Shocked},
                {"=o", ChatEmotion.Shocked},
                {">=o", ChatEmotion.Shocked},

                // Sad
                {";'(", ChatEmotion.Sad},
                {";[", ChatEmotion.Sad},
                {":[", ChatEmotion.Sad},
                {";(", ChatEmotion.Sad},
                {"=(", ChatEmotion.Sad},
                {"='(", ChatEmotion.Sad},
                {"=[", ChatEmotion.Sad},
                {"='[", ChatEmotion.Sad},
                {":(", ChatEmotion.Sad},
                {":-(", ChatEmotion.Sad}
            };
        }

        /// <summary>
        ///     Searches the provided text for any emotions that need to be applied and returns the packet number.
        /// </summary>
        /// <param name="text">The text to search through</param>
        /// <returns></returns>
        public static int GetEmotionsForText(string text)
        {
            text = text.ToLower();
            foreach (var kvp in _mEmotions)
            {
                if (text.Contains(kvp.Key))
                {
                    return GetEmoticonPacketNum(kvp.Value);
                }
            }

            return 0;
        }

        /// <summary>
        ///     Trys to get the packet number for the provided chat emotion.
        /// </summary>
        /// <param name="e">Chat Emotion</param>
        /// <returns></returns>
        private static int GetEmoticonPacketNum(ChatEmotion e)
        {
            switch (e)
            {
                case ChatEmotion.Smile:
                    return 1;

                case ChatEmotion.Angry:
                    return 2;

                case ChatEmotion.Shocked:
                    return 3;

                case ChatEmotion.Sad:
                    return 4;

                default:
                    return 0;
            }
        }
    }
}