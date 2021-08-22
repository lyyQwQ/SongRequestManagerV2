﻿using System.Collections.Generic;

namespace SongRequestManagerV2.Models
{
    public class PlaylistEntity
    {
        public string playlistTitle { get; set; } = "Request History";
        public string playlistAuthor { get; set; } = "SRM V2";
        public string image { get; } = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAgAAAAIACAMAAADDpiTIAAAB+1BMVEUAAACEaL2Far+Gar6Iab+Iar6Iab6Jar6Hab2FZLyIar6Hab2Iar+JacCDbMGHabyIar6HaL9mZsyIa76Ja76OccaLacKIar2Iar6IbMGGab+Iar6Iar6HaL+IacCIar6Jar6Hab6Gab2Jab+AYL+Iar+SbbaGab+Har+Ma72Iar6JaL+Hab+Jbb+JYrqGab2Iar6Gab2Iab6Iar6Jar6Iar6Iar6Iar57WrZ7WrZ5WbR4WbR3V7J3V7FxUqpyUqlqTp1oTpxVQINUP34nGzp7WrcjGzcAAAB0Va11Va1gRo9eRY0gGDB6WbZ6WLYgGC1qTp1pTZ0sHj96WbYpHjxlSpVjSJMJCRJ1Va91Vq8JCQ4zJUt6WbU4KE9UPX57WrdWPnwAAABdRIkAAABfRoxgR5EAAAB6WbYAAAB1VbA2KFEAAAAJCQ5hR5JjSJQAAAAtHjwAAAAAAABfR450Va5fRYwAAAAAAABWP4BxU6l3WLF6WbV3V7AjGzMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACIar6Hab2DZLyBYbp/X7l9Xbh8W7h7WreCY7t+XbiGaL18W7eHar6CYruEZbyDZLuAYLp9XLiMb8D///+tmNLNweTu6vZiSJK9rduwo8hvUaWxpMluUaTXf65WAAAAjHRSTlMAG2CYvt7w+pcXdtHPdSGZ9yAF9IYJLtjXLV/29Vty/f55cHcI1geDfx+ac84cGmFelr+84O/u+vn47eve3L26mppvbUL+QTPOy399QPb1P5yZRfREiYY41dM3S/JNbf1rMnsvfIIs8yXUTB02fo0SRBEFfsx+BCluu9ru2UEoFjEGGRgBKggwFRcaK7sTzb0AAAtuSURBVHja7d0Jf1xVHYfxgVKgKFiVRQEFQUXZUdlFqFAuIBaw7HspQjUstkDZmwBt2UzTtGnTusMdl5epzaTpTGIn58z5n/s/N7/neQF3Pmd+3yS3k8m00yEiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiKildcJJ646afXJp/yRiuqUk1efdOqJJ+Ref81pX/E+KQ3rq6evyTj/GV9b631AWq61q76eaf5vnP5N78NRSGvPPCvH/mef430wCu1b37bf/9zzvE9F4Z3/Hev9v3uB95kopgu/Z7v/Rd4HotguNv3+z9d/67rA8KfA2fz8b2Hnm90Jfp/7/1b2gx8aAbjE+yQ0Wj+y2f8MXv5raef92ATApd7noFE71WL/NXwDaG1rLX4zdLH3KWj0LjMAsPj3v5N7pvZO79tPhbVveu/UnslFY12evv8Vg+vPHPA+KA3rwMyAgQuvTAZwVd/lDk7Neh+Qlmt26mDfZFcnA1i1cK1Dh5m/Fc0ePrQw2jXJAH6y8OU/7X0wCm164ZvAT5MB/Gz+Snv48m9Rs3+an211MoBrexea8T4SxTXT2+26ZADXz13nz97nodh6Aq5PBtD7/u99Goqv91PABMBBfv63sNmDVgAOcf/fyqYPGQE47H0SGq3DNgD4AdDWjvwQMADwF+9z0KhNWQCY5BtAa5udNADAS0AtbsYAAL//bXEH0gFMep+BUppMBsCLgK1uTzKAKe8jUEpTyQD2eh+BUtqbDOCv3keglKaTAfD+31a3LxkALwO1utlkAN4noLQAIB4AxAOAeAAQDwDiAUA8AIgHAPEAIB4AxAOAeAAQDwDiAUA8AIgHAPEAIB4AxAOAeAAQDwDiAUA8AIhXOoC//Z0GUgPwD+8nvLTUAHzh/YSXlhqAL72f8NISA8AtwOLEAHALsDgxANwCLE4MALcAi9MCwC3AkrQAcAuwJC0A3AIsSQsAtwBLkgLALcDSpABwC7A0KQDcAixNCgC3AEtTAhB0C1Cv8JQBBN0CeA8EgHwF3QJ4DwSAfAXdAngPBIBshb0K4D0QALIV9iqA90AAyFbYqwDeAwEgW2GvAngPBIBcBf4iwHsgAOQq8BcB3gMBIFeBvwjwHggAuQr8RYD3QADIVOh7AbwHAkCmQt8L4D0QADIV+l4A74EAkKnQ9wJ4DwSAPAW/HTAXwFJSBRD8dsDMz797qgCC3w6Y+fl3TxVA8NsBMz//7okCCP+LgNwDeCcKIPwvAnIP4J0ogPC/CMg9gHeiAML/IiD3AN5pAoj4o8DsCzinCSDijwKzL+CcJoCIPwrMvoBzmgAi/igw+wLOSQKI+VyA/BP4Jgkg5nMB8k/gmySAmM8FyD+Bb5IAYj4XIP8EvikCiPpooAY2cE0RQNRHAzWwgWuKAKI+GqiBDVxTBBD10UANbOCaIIC4TwdsYgTPBAHEfTpgEyN4Jggg7tMBmxjBM0EAcZ8O2MQInukBiPyA4EZWcEwPQOQHBDeygmN6ACI/ILiRFRzTAxD5AcGNrOCYHIDY/yOgmRn8kgMQ+38ENDODX3IAYv+PgGZm8EsOQOz/EdDMDH7JAYjcP/kJsb5+7utZP98AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAoKzrWT/fAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKCs61k/3wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACgrOtZP98AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAoKzrWT/fAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKCs61k/3wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACgrOtZP98AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAoKzrWT/fAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAArHQA9TJ5Xw8AAAAAAAAAAAAAAAAAAAAAbAMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA0AJQeAAAAAAAAAAAAAAAAAAAAANgGgMIDgDaALgCkAXSznxcAJdfNf14AFFy3gfMCoNy6TZwXAMXWbeS8ACi1bjPnBUChdRs6LwDK7Hj7/9P6gQBQZMfdv7Z+JACU2PH3r60fCgAFNmT/2vqxAFBew/avrR8MAMU1dP/a+tEAUFrD96+tHw4AhbXM/rX14wGgrJbbv7Z+QAAU1bL719aPCICSWn7/2vohAVBQAfvX1o8JgHIK2b+2flAAFFPQ/rX1owKglML2r60fFgCFFLh/bf24ACij0P3/Zf3AACgit/0BUER++wOghBz3B0ABee4PAP9c9weAe777A8A75/0B4Jz3/gDwzX1/ALjmvz8APCtgfwA4VsL+APCriP0B4FYZ+wPAq0L2bz+AzGC66Y8YVdP7A0B8fwCI7w8A8f0BIL4/AMT3B4D4/gAQ3x8A4vsDQHx/AIjvDwDx/QEgvj8AxPcHgPj+ABDfHwDi+wNAfH8AiO8PAPH9ASC+PwDE9weA+P4AEN8fAOL7A0B8fwCI7w8A8f0BIL4/AMT3B4D4/gAQ318egPr+6gDk9xcHwP7aANhfGwD7awNgf20A7K8NgP21AbC/NgD21wbA/toA2F8bAPtrA2B/bQDsrw2g6f3/Xfb+egAarvCvfwCo7w8A8f0BIL4/AMT3B4D4/gAQ3x8A4vsDQHx/AIjvDwDx/QEgvj8AxPcHgPj+ABDfHwC59/+P98IAcN2/8K9/AKjvDwDx/QEgvj8AxPcHgPj+ABDfHwDi+wNAfH8AiO8PAPH9ASC+PwDE9weA+P4AEN8fAOL7lwegTsx4z9jatj8AxPcHgPj+ABDfHwDi+wNAfH8AiO8PAPH9ASC+PwDE9weA+P4AEN8fAOL7A0B8fwCI7w8A8f3LA5Ba4v5t3zM6cQDy+4sDYH9tAOyvDYD990sDYP8j6QJg/7lkAbB/L1UA7D+fKAD2P5omAPZfSBIA+x9LEQD79yUIgP370wPA/gPJAWD/wVYcALH3cyQnDkB+f3EA7K8NgP21AbD/fmkA7H8kXQDsP5csAPbvpQqA/ecTBcD+R9MEwP4LSQJg/2MpAmD/vgQBsH9/egDYfyA5AOw/2IoDQHEBQDwAiAcA8QAgHgDEA4B4ABAvGcAN3ieglG5MBnCT9xEopZuTAfzc+wiU0i3JAH7hfQRK6dZkALd5H4FSWpcM4JfeR6CUbk8GcMd67zPQ6K2/MxlAdZf3IWj07q7SAdzjfQgavV8ZALj3196noFHbcJ8BgOp+72PQqD1QWQD4zQbvc9BobXzQBED1kPdBaLQermwAPPKo90lolB573AhA9cST3meh+DY+VVkBqJ5+xvs0FNszz1YmADbNXeY57+NQbPfMDbcpGcDmuetUz/M9oFWt/21vt83JAF7oXah6eqP3mSi8F5+dn+2FZABb5q9UPfU771NRaI/9/uhqW5IBjB29VPXSyy96H4xC2vDKIwujjSUDeLU61h9u4/cCxbdh3YN9k21NBrCt6u/e1173PiANaf3db9w3MNj2ZACdN6vB7nxr3dvvvMu7xQvrhpvee/uB299fNNaO9P074xW1tnEDANsnvE9BozZh8BOg/98B1LLS/w1wpA/4FtDSJraZAOh86H0QGq2PbPbv7NzlfRIapV07jQB0dn/sfRaKb2K31f6dzlbvw1B86S8C9vWJ92koNouXABDQ3j613f9/PwX4x2CLmjD9/t9rN/8WaE27DO//jrVznG8CrWjiM7N//y3qgzEIFN/EmNHrf/+37eM7vA9Iw9oxbvL7n2Fte3Vsy+cfb/I+KQ22afPnW8a2Zl+fiIiIiIiIiIiIiIiIiIiIWtB/AbxmE3m92gO2AAAAAElFTkSuQmCC";
        public List<PlaylistSongEntity> songs { get; set; } = new List<PlaylistSongEntity>();
    }
}
