﻿
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonkeyArms;
using WhitePaperBible.Core.Views;
using WhitePaperBible.Core.Models;
using WhitePaperBible.iOS.Invokers;
using WhitePaperBible.Core.Invokers;
using ElementPack;
using CustomElements;
using System.Drawing;
using IOS.Util;

namespace WhitePaperBible.iOS
{
	public partial class EditPaperView : DialogViewController, IEditPaperView, IMediatorTarget
	{
		EntryElement TitleEl;

		SimpleMultilineEntryElement DescriptionEl;

		public Invoker Save {
			get;
			private set;
		}

		public Invoker Delete {
			get;
			private set;
		}

		public EditPaperView (Paper paper=null) : base (UITableViewStyle.Grouped, null)
		{
			Save = new Invoker ();
			Delete = new Invoker ();
			if(paper != null){
				SetPaper (paper);
			}

		}

		public List<Tag> Tags {
			get;
			set;
		}

		List<Reference> GetReferences ()
		{
			var refs = new List<Reference> ();
			foreach(var el in VerseEls){
				refs.Add (new Reference () {
					reference = el.Value
				});
			}

			return refs;
		}

		List<Tag> GetTags ()
		{
			string[] t = TagsEl.Value.Split (',');
			var tags = new List<Tag> ();
			foreach(var s in t){
				tags.Add (new Tag (){ name = s });
			}

			return tags;

		}

		#region IEditPaperView implementation

		LoginRequiredController LoginRequiredView;

		Section VersesSection;
		List<VerseEntryElement> VerseEls;

		StyledStringElement TagsEl;

		public void SetPaper (Paper paper)
		{
			if(VerseEls != null){
				return; // don't let it wipe edits that haven't been committed
			}

			var AddVerseEl = new StyledStringElement("Add Verse"){
				Image = UIImage.FromBundle("plus_alt")
			};
			AddVerseEl.Tapped += () => {
				var entryElement = new VerseEntryElement ("", "Verse", "");
				VerseEls.Add(entryElement);
				VersesSection.Insert (1, UITableViewRowAnimation.Left, entryElement);
			};

			VerseEls = new List<VerseEntryElement> ();
			VersesSection = new Section ("Verses");
			VersesSection.Add (AddVerseEl);

			if (paper.references != null) {
				foreach (var r in paper.references) {
					var entryElement = new VerseEntryElement ("", "Verse", r.reference);
					VersesSection.Add (entryElement);
					VerseEls.Add(entryElement);
				}
			}

			var ActionsSection = new Section ("");

			Root = new RootElement ("Edit Paper") {
				new Section ("") {
					(TitleEl = new EntryElement ("", "Title", paper.title)),
					(DescriptionEl = new SimpleMultilineEntryElement ("", "Description", paper.description)),
					(TagsEl = new StyledStringElement ("Tags","...") { 
						Accessory = UITableViewCellAccessory.DisclosureIndicator 
					})
				},
				VersesSection,
				ActionsSection
			};

			if(paper.id > 0){
				ActionsSection.Add (new StyledStringElement ("Delete Paper", () => Delete.Invoke ()){ TextColor = UIColor.Red});
			}

			TagsEl.Tapped += () => {
				var tagsView = new  PaperTagsView();
				tagsView.Controller = this;
				((UINavigationController)this.ParentViewController).PushViewController(tagsView, true);
			};

			if(paper.tags != null && paper.tags.Count > 0){
				Tags = paper.tags;
			}



		}

		public void PromptForLogin ()
		{
			if (LoginRequiredView == null) {
				CreateLoginRequiredView ();
				LoginRequiredView.View.Hidden = false;
			}
		}

		public void ShowLoginForm ()
		{
			var loginView = new LoginViewController ();
			loginView.LoginFinished.Invoked += (object sender, EventArgs e) => {
				(e as LoginFinishedInvokerArgs).Controller.DismissViewController (true, null);
			};

			this.PresentViewController (loginView, true, null);
		}

		public void DismissLoginPrompt()
		{
			if (LoginRequiredView != null && !LoginRequiredView.View.Hidden) {
				LoginRequiredView.View.Hidden = true;
			}
		}

		protected void CreateLoginRequiredView ()
		{
			LoginRequiredView = new LoginRequiredController ();
//			LoginRequiredView.Frame = new RectangleF (0, 48, View.Bounds.Width, View.Bounds.Height);

			View.AddSubview (LoginRequiredView.View);
			View.BringSubviewToFront (LoginRequiredView.View);
			LoginRequiredView.LoginRegister.Invoked += (object sender, EventArgs e) => ShowLoginForm ();
			LoginRequiredView.CancelRegister.Invoked += (object sender, EventArgs e) => {
//				this.ParentViewController.DismissViewController (true, null);
				this.ParentViewController.NavigationController.DismissViewController(true, null);
			};
			LoginRequiredView.View.Hidden = true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return base.GetSupportedInterfaceOrientations ();
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{

//			if(LoginRequiredView != null){
//				LoginRequiredView.Frame = new RectangleF (0, 48, View.Bounds.Width, View.Bounds.Height);
//			}
			base.DidRotate (fromInterfaceOrientation);
		}

		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
		}

		public void DismissController (bool deleted)
		{
			InvokeOnMainThread (() => {
				((UINavigationController)this.ParentViewController).DismissViewController (true, null);
			});


		}

		#endregion

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem ("Save", UIBarButtonItemStyle.Plain, (sender, args)=> {
					var paper = new Paper(){
						title = TitleEl.Value,
						description = DescriptionEl.Value,
						references = GetReferences(),
						tags = GetTags()
					};

					var invokerArgs = new SavePaperInvokerArgs(paper);
					Save.Invoke(invokerArgs);
				})
				, true
			);

			NavigationItem.SetLeftBarButtonItem (
				new UIBarButtonItem ("Cancel", UIBarButtonItemStyle.Plain, (sender, args)=> {
					DismissController(false);
				})
				, true
			);

			AnalyticsUtil.TrackScreen (this.Title);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			DI.RequestMediator (this);

			if(Tags != null && Tags.Count > 0){
				string[] tags = Tags.Select (x => x.name).ToArray ();
				TagsEl.Value = string.Join (",", tags);
			}

			this.TableView.ReloadData ();
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			DI.DestroyMediator (this);
		}

		// This is our subclass of the fixed-size Source that allows editing
		public class EditingSource : DialogViewController.SizingSource {
			public EditingSource (DialogViewController dvc) : base (dvc) {}

			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				// Trivial implementation: we let all rows be editable, regardless of section or row
				if (indexPath.Section == 0 || (indexPath.Section == 1 && indexPath.Row == 0) || indexPath.Section == 2) {
					return false;
				}else{
					return true;
				}
			}

			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				// trivial implementation: show a delete button always
				if (indexPath.Section == 0 || (indexPath.Section == 1 && indexPath.Row == 0) || indexPath.Section == 2) {
					return UITableViewCellEditingStyle.None;
				}else{
					return UITableViewCellEditingStyle.Delete;
				}
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				//
				// In this method, we need to actually carry out the request
				//
				var section = Container.Root [indexPath.Section];
				var element = section [indexPath.Row] as VerseEntryElement;
				element.Delete ();
				section.Remove (element);
			}
		}

		public override Source CreateSizingSource (bool unevenRows)
		{
//			if (unevenRows)
//				throw new NotImplementedException ("You need to create a new SourceSizing subclass, this sample does not have it");
//			return (!unevenRows) ? new DialogViewController.Source (this) : new DialogViewController.SizingSource (this);
			return new EditingSource (this);
		}
	}
}