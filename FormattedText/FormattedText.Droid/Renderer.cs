using System.ComponentModel;
using System.Reflection;
using Android.Graphics;
using Android.Text;
using FormattedText.Droid;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


//[assembly: ExportRenderer(typeof(Label), typeof(SimpleLabelRenderer))]
[assembly: ExportRenderer(typeof(Label), typeof(FormattedLabelRenderer))]
namespace FormattedText.Droid
{
    public class SimpleLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.Typeface = Typeface.CreateFromAsset(Forms.Context.Assets, GetFontName(Element.FontFamily, Element.FontAttributes));
            }
        }

        private static string GetFontName(string fontFamily, FontAttributes fontAttributes)
        {
            var postfix = "Regular";
            var bold = fontAttributes.HasFlag(FontAttributes.Bold);
            var italic = fontAttributes.HasFlag(FontAttributes.Italic);
            if (bold && italic) { postfix = "BoldItalic"; }
            else if (bold) { postfix = "Bold"; }
            else if (italic) { postfix = "Italic"; }

            return $"{fontFamily}-{postfix}.ttf";
        }
    }

    public class FormattedLabelRenderer : SimpleLabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            UpdateFormattedText();
        }

        private void UpdateFormattedText()
        {
            if (Element?.FormattedText == null)
                return;

            var extensionType = typeof(FormattedStringExtensions);
            var type = extensionType.GetNestedType("FontSpan", BindingFlags.NonPublic);
            var ss = new SpannableString(Control.TextFormatted);
            var spans = ss.GetSpans(0, ss.ToString().Length, Class.FromType(type));
            foreach (var span in spans)
            {
                var start = ss.GetSpanStart(span);
                var end = ss.GetSpanEnd(span);
                var flags = ss.GetSpanFlags(span);
                var font = (Font)type.GetProperty("Font").GetValue(span, null);
                ss.RemoveSpan(span);
                var newSpan = new CustomTypefaceSpan(Control, Element, font);
                ss.SetSpan(newSpan, start, end, flags);
            }
            Control.TextFormatted = ss;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Label.FormattedTextProperty.PropertyName ||
                e.PropertyName == Label.TextProperty.PropertyName ||
                e.PropertyName == Label.FontAttributesProperty.PropertyName ||
                e.PropertyName == Label.FontProperty.PropertyName ||
                e.PropertyName == Label.FontSizeProperty.PropertyName ||
                e.PropertyName == Label.FontFamilyProperty.PropertyName ||
                e.PropertyName == Label.TextColorProperty.PropertyName)
            {
                UpdateFormattedText();
            }
        }
    }
}