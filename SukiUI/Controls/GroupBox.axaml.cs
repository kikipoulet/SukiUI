using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace SukiUI.Controls
{
    [TemplatePart("PART_HeaderPresenter", typeof(ContentPresenter), IsRequired = true)]
    [TemplatePart("PART_ContentPresenter", typeof(ContentPresenter), IsRequired = true)]
    public class GroupBox : HeaderedContentControl
    {
        public GroupBox()
        {
        }
    }
}