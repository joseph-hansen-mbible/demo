using System.Linq;
using NaughtyAttributes;
using Turnroot.Characters;
using Turnroot.Characters.Subclasses;
using UnityEngine;
using UnityEngine.Events;

namespace TurnrootFramework.Conversations
{
    [System.Serializable]
    public class ConversationLayer
    {
        [SerializeField]
        private string _dialogue;
        private string _parsedDialogue;

        private bool _hasBeenParsed;

        public bool HasBeenParsed
        {
            get => _hasBeenParsed;
            set => _hasBeenParsed = value;
        }
        public string Dialogue
        {
            get => _dialogue;
            set => _dialogue = value;
        }

        public string ParsedDialogue
        {
            get
            {
                if (_parsedDialogue == null)
                    ParseDialogue();
                return _parsedDialogue;
            }
        }

        [SerializeField, SerializeReference]
        private CharacterData _speaker;
        public CharacterData Speaker
        {
            get => _speaker;
            set
            {
                _speaker = value;
                // Clear portrait key if speaker changes and key is invalid
                if (
                    _speaker == null
                    || (
                        _speakerPortraitKey != null
                        && !_speaker.Portraits.ContainsKey(_speakerPortraitKey)
                    )
                )
                {
                    _speakerPortraitKey = null;
                }
            }
        }

        [SerializeField]
        private bool _parseProunouns = true;
        public bool ParsePronouns
        {
            get => _parseProunouns;
            set => _parseProunouns = value;
        }

        [SerializeField, ShowIf(nameof(ParsePronouns))]
        private CharacterData[] _referringTo;
        public CharacterData[] ReferringTo
        {
            get => _referringTo;
            set => _referringTo = value;
        }

        [SerializeField]
        private string _speakerDisplayName;
        public string SpeakerDisplayName
        {
            get => _speakerDisplayName;
            set => _speakerDisplayName = value;
        }

        [SerializeField, Dropdown("GetAvailablePortraitKeys")]
        private string _speakerPortraitKey;

        public string SpeakerPortraitKey
        {
            get => _speakerPortraitKey;
            set
            {
                _speakerPortraitKey = value;
                // Clear cached sprite when portrait changes
                _PortraitSprite = null;
            }
        }

        public Portrait SpeakerPortrait
        {
            get
            {
                if (
                    _speaker != null
                    && _speakerPortraitKey != null
                    && _speaker.Portraits.ContainsKey(_speakerPortraitKey)
                )
                {
                    return _speaker.Portraits[_speakerPortraitKey];
                }
                return null;
            }
        }

        public UnityEvent OnLayerStart;
        public UnityEvent OnLayerEnd;
        public UnityEvent OnLayerComplete;
        private Sprite _PortraitSprite;

        public Sprite PortraitSprite
        {
            get
            {
                if (_PortraitSprite == null && SpeakerPortrait != null)
                {
                    _PortraitSprite = SpeakerPortrait.SavedSprite;
                }
                return _PortraitSprite;
            }
        }

        public void OnAwake()
        {
            if (SpeakerPortrait != null)
            {
                _PortraitSprite = SpeakerPortrait.SavedSprite;
            }
        }

        public void StartLayer()
        {
            OnLayerStart?.Invoke();
        }

        public void EndLayer()
        {
            OnLayerEnd?.Invoke();
        }

        public void CompleteLayer()
        {
            OnLayerComplete?.Invoke();
        }

        public void ParseDialogue()
        {
            Debug.Log("Parsing pronouns");
            _parsedDialogue = ParsePronounsInDialogue();
            _hasBeenParsed = true;
        }

        public string ParsePronounsInDialogue()
        {
            string _t = Dialogue;
            string[] pronouns = { "they", "them", "their", "theirs" };

            if (ParsePronouns && ReferringTo != null)
            {
                // find instances of #1{them} or #fred{them}
                // where 1 is the index of the character in ReferringTo
                // or fred is the name of the character
                // and run Pronouns.Use() on the {them} part

                // first the #1 pass
                for (int i = 0; i < ReferringTo.Length; i++)
                {
                    var character = ReferringTo[i];
                    if (character != null)
                    {
                        // Replace each pronoun type for this character
                        foreach (string pronoun in pronouns)
                        {
                            string placeholder = $"#{i + 1}{{{pronoun}}}";
                            _t = _t.Replace(
                                placeholder,
                                character.CharacterPronouns.Use($"{{{pronoun}}}")
                            );
                        }
                    }
                }

                // then the #fred pass
                for (int i = 0; i < ReferringTo.Length; i++)
                {
                    var character = ReferringTo[i];
                    if (character != null)
                    {
                        string namePlaceholder = $"#{character.DisplayName.ToLower()}";
                        foreach (string pronoun in pronouns)
                        {
                            string placeholder = $"{namePlaceholder}{{{pronoun}}}";
                            _t = _t.Replace(
                                placeholder,
                                character.CharacterPronouns.Use($"{{{pronoun}}}")
                            );
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No pronouns to parse were found");
                _t = Dialogue;
            }
            return _t;
        }

        private bool HasSpeaker()
        {
            return _speaker != null;
        }

        private string[] GetAvailablePortraitKeys()
        {
            if (_speaker == null || _speaker.Portraits == null)
            {
                // Clear the key if speaker is null
                if (!string.IsNullOrEmpty(_speakerPortraitKey))
                {
                    _speakerPortraitKey = null;
                }
                return new string[] { "No speaker selected" };
            }

            var keys = _speaker.Portraits.Keys.ToArray();
            // Ensure current value is valid
            if (!string.IsNullOrEmpty(_speakerPortraitKey) && !keys.Contains(_speakerPortraitKey))
            {
                _speakerPortraitKey = null;
            }
            return keys.Length > 0 ? keys : new string[] { "No portraits available" };
        }
    }
}
