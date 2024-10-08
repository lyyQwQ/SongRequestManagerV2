﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SongRequestManagerV2.Utils
{
    public class StringNormalization
    {
        public static HashSet<string> BeatsaverBadWords = new HashSet<string>();

        public void ReplaceSymbols(StringBuilder text, char[] mask)
        {
            for (var i = 0; i < text.Length; i++) {
                var c = text[i];
                if (c < 128) {
                    text[i] = mask[c];
                }
            }
        }

        public string RemoveSymbols(string text, char[] mask)
        {
            var o = new StringBuilder(text.Length);
            foreach (var c in text) {
                if (c > 127 || mask[c] != ' ') {
                    o.Append(c);
                }
            }
            return o.ToString();
        }

        public string RemoveDirectorySymbols(string text)
        {
            var mask = this.SymbolsValidDirectory;
            var o = new StringBuilder(text.Length);

            foreach (var c in text) {
                if (c > 127 || mask[c] != '\0') {
                    o.Append(c);
                }
            }
            return o.ToString();
        }

        // This function takes a user search string, and fixes it for beatsaber.
        public string NormalizeBeatSaverString(string text)
        {
            var words = this.Split(text);
            var result = new StringBuilder();
            foreach (var word in words) {
                if (word.Length < 3) {
                    continue;
                }

                if (BeatsaverBadWords.Contains(word.ToLower())) {
                    continue;
                }

                result.Append(word);
                result.Append(' ');
            }

            //RequestBot.Instance.QueueChatMessage($"Search string: {result.ToString()}");


            if (result.Length == 0) {
                return "qwesartysasasdsdaa";
            }

            return result.ToString().Trim();
        }

        public string[] Split(string text)
        {
            var sb = new StringBuilder(text);
            this.ReplaceSymbols(sb, this.SymbolsMap);
            var result = sb.ToString().ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return result;
        }

        public char[] SymbolsMap { get; } = new char[128];
        public char[] SymbolsNoDash { get; } = new char[128];
        public char[] SymbolsValidDirectory { get; } = new char[128];

        public StringNormalization()
        {
            for (var i = (char)0; i < 128; i++) {
                this.SymbolsMap[i] = i;
                this.SymbolsNoDash[i] = i;
                this.SymbolsValidDirectory[i] = i;
            }

            foreach (var c in new char[] { '@', '*', '+', ':', '-', '<', '~', '>', '(', ')', '[', ']', '/', '\\', '.', ',' }) {
                if (c < 128) {
                    this.SymbolsMap[c] = ' ';
                }
            }

            foreach (var c in new char[] { '@', '*', '+', ':', '<', '~', '>', '(', ')', '[', ']', '/', '\\', '.', ',' }) {
                if (c < 128) {
                    this.SymbolsNoDash[c] = ' ';
                }
            }

            foreach (var c in Path.GetInvalidPathChars()) {
                if (c < 128) {
                    this.SymbolsValidDirectory[c] = '\0';
                }
            }

            this.SymbolsValidDirectory[':'] = '\0';
            this.SymbolsValidDirectory['\\'] = '\0';
            this.SymbolsValidDirectory['/'] = '\0';
            this.SymbolsValidDirectory['+'] = '\0';
            this.SymbolsValidDirectory['*'] = '\0';
            this.SymbolsValidDirectory['?'] = '\0';
            this.SymbolsValidDirectory[';'] = '\0';
            this.SymbolsValidDirectory['$'] = '\0';
            this.SymbolsValidDirectory['.'] = '\0';
            this.SymbolsValidDirectory['('] = '\0';
            this.SymbolsValidDirectory[')'] = '\0';

            // Incomplete list of words that BeatSaver.com filters out for no good reason. No longer applies!
            foreach (var word in new string[] { "pp" }) {
                BeatsaverBadWords.Add(word.ToLower());
            }
        }
    }
}
