using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Security;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Enable. This class cannot be inherited.
    /// </summary>
    internal sealed class AddBlackWord : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AddBlackWord" /> class.
        /// </summary>
        public AddBlackWord()
        {
            MinRank = 8;
            Description = "Adds a word to filter list.";
            Usage = ":addblackword word";
            MinParams = 1;
            BlockBad = true;

        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var word = pms[0];

            if (string.IsNullOrEmpty(word))
            {
                session.SendWhisper("Palabra inválida.");
                return true;
            }
            BobbaFilter.AddBlackWord(word);
            return true;
        }
    }
}