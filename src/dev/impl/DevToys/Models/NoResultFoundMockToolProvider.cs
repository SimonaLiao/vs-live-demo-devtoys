#nullable enable

using System;
using System.ComponentModel;

using DevToys.Api.Tools;

namespace DevToys.Models
{
    internal sealed class NoResultFoundMockToolProvider : IToolProvider
    {
        public string MenuDisplayName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string? SearchDisplayName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string? Description => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string AccessibleName => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string? SearchKeywords => LanguageManager.Instance.MainPage.SearchNoResultsFound;

        public string IconGlyph => null!;

#pragma warning disable 0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 0067

        public bool CanBeTreatedByTool(string data)
        {
            return false;
        }

        public IToolViewModel CreateTool()
        {
            throw new NotSupportedException();
        }
    }
}
