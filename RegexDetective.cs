using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Seq.Apps;

namespace Seq.App.Detective
{
    [SeqApp("Regular expression detector",
        Description = "Checks complete event documents for text matching a regular expression.")]
    public class RegexDetective : SeqApp, ISubscribeToJsonAsync
    {
        const int MaximumFragmentChars = 4096;

        [SeqAppSetting(HelpText = "A regular expression to search for.")]
        public string Pattern { get; set; }

        public Task OnAsync(string json)
        {
            var matches = Regex.Matches(json, Pattern);

            if (matches.Count > 0)
            {
                var positions = matches.Cast<Match>()
                    .Select(m => m.Index)
                    .ToArray();

                var sanitized = Regex.Replace(json, Pattern, m => new string('#', m.Value.Length));
                var fragment = sanitized.Substring(0, Math.Min(sanitized.Length, MaximumFragmentChars));

                Log.ForContext("Pattern", Pattern)
                    .ForContext("Fragment", fragment)
                    .ForContext("Positions", positions, destructureObjects: true)
                    .Error("Match detected");
            }

            return Task.CompletedTask;
        }
    }
}
