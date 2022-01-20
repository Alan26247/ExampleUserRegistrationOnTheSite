using System;
using System.Text;

namespace Site.Sources
{
    public class PasswordGenerator
    {
        private readonly Random Random = new();

        public string Generate(int countСharacters)
        {
            StringBuilder returnString = new (countСharacters);

            for (int i = 0; i < countСharacters; i++)
            {
                returnString.Append(GetCharacter());
            }

            return returnString.ToString();
        }

        private string GetCharacter()
        {
            const int countCharacter = 62;
            int numberCharacter = Random.Next(countCharacter);

            if (numberCharacter == 0) return "A";
            else if (numberCharacter == 1) return "B";
            else if (numberCharacter == 2) return "C";
            else if (numberCharacter == 3) return "D";
            else if (numberCharacter == 4) return "E";
            else if (numberCharacter == 5) return "F";
            else if (numberCharacter == 6) return "G";
            else if (numberCharacter == 7) return "H";
            else if (numberCharacter == 8) return "I";
            else if (numberCharacter == 9) return "J";
            else if (numberCharacter == 10) return "K";
            else if (numberCharacter == 11) return "L";
            else if (numberCharacter == 12) return "M";
            else if (numberCharacter == 13) return "N";
            else if (numberCharacter == 14) return "O";
            else if (numberCharacter == 15) return "P";
            else if (numberCharacter == 16) return "Q";
            else if (numberCharacter == 17) return "R";
            else if (numberCharacter == 18) return "S";
            else if (numberCharacter == 19) return "T";
            else if (numberCharacter == 20) return "U";
            else if (numberCharacter == 21) return "V";
            else if (numberCharacter == 22) return "W";
            else if (numberCharacter == 23) return "X";
            else if (numberCharacter == 24) return "Y";
            else if (numberCharacter == 25) return "Z";
            else if (numberCharacter == 26) return "a";
            else if (numberCharacter == 27) return "b";
            else if (numberCharacter == 28) return "c";
            else if (numberCharacter == 29) return "d";
            else if (numberCharacter == 30) return "e";
            else if (numberCharacter == 31) return "f";
            else if (numberCharacter == 32) return "g";
            else if (numberCharacter == 33) return "h";
            else if (numberCharacter == 34) return "i";
            else if (numberCharacter == 35) return "j";
            else if (numberCharacter == 36) return "k";
            else if (numberCharacter == 37) return "l";
            else if (numberCharacter == 38) return "m";
            else if (numberCharacter == 39) return "n";
            else if (numberCharacter == 40) return "o";
            else if (numberCharacter == 41) return "p";
            else if (numberCharacter == 42) return "q";
            else if (numberCharacter == 43) return "r";
            else if (numberCharacter == 44) return "s";
            else if (numberCharacter == 45) return "t";
            else if (numberCharacter == 46) return "u";
            else if (numberCharacter == 47) return "v";
            else if (numberCharacter == 48) return "w";
            else if (numberCharacter == 49) return "x";
            else if (numberCharacter == 50) return "y";
            else if (numberCharacter == 51) return "z";
            else if (numberCharacter == 52) return "0";
            else if (numberCharacter == 53) return "1";
            else if (numberCharacter == 54) return "2";
            else if (numberCharacter == 55) return "3";
            else if (numberCharacter == 56) return "4";
            else if (numberCharacter == 57) return "5";
            else if (numberCharacter == 58) return "6";
            else if (numberCharacter == 59) return "7";
            else if (numberCharacter == 60) return "8";
            else if (numberCharacter == 61) return "9";

            return "0";
        }
    }
}
