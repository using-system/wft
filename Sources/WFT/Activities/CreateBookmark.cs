using System.Activities;
using System.ComponentModel;
using System.Drawing;
using WFT.Activities.Designers;

namespace WFT.Activities
{
    /// <summary>
    /// Create a bookmark
    /// </summary>
    [Description("Create a bookmark")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.BookmarkToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/BookmarkDesigner.bmp")]
    public sealed class CreateBookmark : CreateBookmark<object>
    {

    }

    /// <summary>
    /// Create a bookmark with a return value
    /// </summary>
    /// <typeparam name="T">Type of the return value</typeparam>
    [Description("Create a bookmark with a return value")]
    [ToolboxBitmapAttribute(typeof(EntryPoint), "Resources.BookmarkToolbox.bmp")]
    [Designer(typeof(IconActivityDesigner))]
    [DesignerIcon("Resources/BookmarkDesigner.bmp")]
    public class CreateBookmark<T> : NativeActivity<T>
    {
        /// <summary>
        /// Bookmark name
        /// <remarks>
        /// Required Argument
        /// </remarks>
        /// </summary>
        [RequiredArgument]
        [Description("Bookmark name")]
        public InArgument<string> BookmarkName { get; set; }

        /// <summary>
        /// Bookmark options
        /// </summary>
        [Description("Bookmark options")]
        public BookmarkOptions BookmarkOptions { get; set; }

        public CreateBookmark()
        {
            BookmarkOptions = System.Activities.BookmarkOptions.None;
        }


        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            string bookmarkName = BookmarkName.Get(context);

            context.CreateBookmark(bookmarkName, new BookmarkCallback(OnBookmarkCallback));
        }

        void OnBookmarkCallback(NativeActivityContext context, Bookmark bookmark, object state)
        {
            Result.Set(context, (T)state);
        }
    }
}
