using Oblivion.HabboHotel.Commands.Interfaces;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Security;

namespace Oblivion.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Enable. This class cannot be inherited.
    /// </summary>
    internal sealed class DeleteBlackWord : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeleteBlackWord" /> class.
        /// </summary>
        public DeleteBlackWord()
        {
            MinRank = 8;
            Description = "Delete a word from filter list.";
            Usage = ":deleteblackword word";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var word = pms[0];

            if (string.IsNullOrEmpty(word))
            {
                session.SendWhisper("Palabra inválida.");
                return true;
            }
            BobbaFilter.DeleteBlackWord(word);
            return true;
        }
    }
}