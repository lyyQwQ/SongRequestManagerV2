using BeatSaberMarkupLanguage.ViewControllers;
using SongRequestManagerV2.Bots;
using SongRequestManagerV2.Interfaces;
using SongRequestManagerV2.Localizes;
using UnityEngine;
using Zenject;

namespace SongRequestManagerV2.Views
{
    public class KeyboardViewController : BSMLViewController
    {
        [Inject]
        private readonly Keyboard.KEYBOARDFactiry _factiry;
        [Inject]
        private readonly IRequestBot _bot;

        public override string Content => @"<bg></bg>";

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation) {
                var KeyboardContainer = new GameObject("KeyboardContainer", typeof(RectTransform)).transform as RectTransform;
                KeyboardContainer.SetParent(this.rectTransform, false);
                KeyboardContainer.sizeDelta = new Vector2(60f, 40f);

                var mykeyboard = this._factiry.Create().Setup(KeyboardContainer, "");
                _ = mykeyboard.AddKeys(Keyboard.QWERTY); // You can replace this with DVORAK if you like
                _ = mykeyboard.DefaultActions();
                var clearSearch = ResourceWrapper.Get("BUTTON_CLEAR_SEARCH");
                var newest = ResourceWrapper.Get("BUTTON_NEWEST");
                var ranked = ResourceWrapper.Get("BUTTON_RANKED");
                var unfiltered = ResourceWrapper.Get("BUTTON_UNFILTERED");
                var search = ResourceWrapper.Get("BUTTON_SEARCH");
                var searchButtons = $@"

[{clearSearch}]/0 /2 [{newest}]/0 /2 [{ranked}]/0 /2 [{unfiltered}]/30 /2 [{search}]/0";

                mykeyboard.SetButtonType("OkButton"); // Adding this alters button positions??! Why?
                _ = mykeyboard.AddKeys(searchButtons, 0.75f);

                mykeyboard.SetAction(clearSearch, this._bot.ClearSearch);
                mykeyboard.SetAction(unfiltered, this._bot.UnfilteredSearch);
                mykeyboard.SetAction(search, this._bot.Search);
                mykeyboard.SetAction(ranked, this._bot.PP);
                mykeyboard.SetAction(newest, this._bot.Newest);
                // The UI for this might need a bit of work.
                mykeyboard.AddKeyboard("RightPanel.kbd");
            }
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }
    }
}
