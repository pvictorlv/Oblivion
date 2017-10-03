using System.Collections.Generic;
using System.Linq;

namespace Oblivion.HabboHotel.Misc
{
    /// <summary>
    ///     Class AntiMutant.
    /// </summary>
    internal class AntiMutant
    {
        /// <summary>
        ///     The _parts
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, Figure>> _parts;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AntiMutant" /> class.
        /// </summary>
        public AntiMutant() => _parts = new Dictionary<string, Dictionary<string, Figure>>();

        /// <summary>
        ///     Runs the look.
        /// </summary>
        /// <param name="look">The look.</param>
        /// <returns>System.String.</returns>
        internal string RunLook(string look)
        {
            var toReturnFigureParts = new List<string>();
            var fParts = new List<string>();
            string[] requiredParts = {"hd", "ch"};
            var flagForDefault = false;

            var figureParts = look.Split('.');
            var genderLook = GetLookGender(look);
            foreach (var part in figureParts)
            {
                var newPart = part;
                var tPart = part.Split('-');
                if (tPart.Length < 2)
                {
                    flagForDefault = true;
                    continue;
                }
                var partName = tPart[0];
                var partId = tPart[1];
                if (!_parts.ContainsKey(partName) || !_parts[partName].ContainsKey(partId) ||
                    genderLook != "U" && _parts[partName][partId].Gender != "U" &&
                    _parts[partName][partId].Gender != genderLook)
                    newPart = SetDefault(partName, genderLook);
                if (!fParts.Contains(partName))
                    fParts.Add(partName);
                if (!toReturnFigureParts.Contains(newPart))
                    toReturnFigureParts.Add(newPart);
            }

            if (flagForDefault)
            {
                toReturnFigureParts.Clear();
                toReturnFigureParts.AddRange("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62".Split('.'));
            }

            foreach (var requiredPart in requiredParts.Where(requiredPart => !fParts.Contains(requiredPart) &&
                                                                             !toReturnFigureParts.Contains(
                                                                                 SetDefault(requiredPart, genderLook)))
            )
                toReturnFigureParts.Add(SetDefault(requiredPart, genderLook));
            return string.Join(".", toReturnFigureParts);
        }

        /// <summary>
        ///     Gets the look gender.
        /// </summary>
        /// <param name="look">The look.</param>
        /// <returns>System.String.</returns>
        private string GetLookGender(string look)
        {
            var figureParts = look.Split('.');

            foreach (var part in figureParts)
            {
                var tPart = part.Split('-');
                if (tPart.Length < 2)
                    continue;
                var partName = tPart[0];
                var partId = tPart[1];
                if (partName != "hd")
                    continue;
                return _parts.ContainsKey(partName) && _parts[partName].ContainsKey(partId)
                    ? _parts[partName][partId].Gender
                    : "U";
            }
            return "U";
        }

        /// <summary>
        ///     Sets the default.
        /// </summary>
        /// <param name="partName">Name of the part.</param>
        /// <param name="gender">The gender.</param>
        /// <returns>System.String.</returns>
        private string SetDefault(string partName, string gender)
        {
            var partId = "0";
            if (!_parts.ContainsKey(partName))
                return $"{partName}-{partId}-0";
            var part = _parts[partName].FirstOrDefault(x => x.Value.Gender == gender || gender == "U");
            partId = part.Equals(default(KeyValuePair<string, Figure>)) ? "0" : part.Key;
            return $"{partName}-{partId}-0";
        }
    }
}