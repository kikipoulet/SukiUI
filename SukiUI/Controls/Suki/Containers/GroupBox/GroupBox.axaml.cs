using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace SukiUI.Controls
{
    [TemplatePart(Name = "PART_Root", Type = typeof(Border))]
    [TemplatePart("PART_HeaderPresenter", typeof(ContentPresenter), IsRequired = true)]
    [TemplatePart("PART_ContentPresenter", typeof(ContentPresenter), IsRequired = true)]
    public class GroupBox : HeaderedContentControl
    {
        public GroupBox()
        {
        }
    }
}