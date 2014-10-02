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
using WhitePaperBible.Core.Invokers;
using IOS.Util;

namespace WhitePaperBible.iOS
{
	public partial class RegistrationView : DialogViewController, IRegistrationView, IMediatorTarget
	{
		EntryElement FullNameEl;

		EntryElement EmailEl;

		EntryElement UsernameEl;

		EntryElement PasswordEl;

		EntryElement PasswordConfirmEl;

		public RegistrationView () : base (UITableViewStyle.Grouped, null)
		{
			Register = new Invoker ();


			Root = new RootElement ("Create Account") {
				new Section ("") {
					(FullNameEl = new EntryElement ("Full Name", "Jane Doe", String.Empty)),
					(EmailEl = new EntryElement ("Email", "jane@doe.com", String.Empty)),
					(UsernameEl = new EntryElement ("Username", "janedoe", String.Empty)),
					(PasswordEl = new EntryElement ("Password", "****", String.Empty, true)),
					(PasswordConfirmEl = new EntryElement ("Password Confirm", "****", String.Empty, true)),
					new StringElement("By registering you agree to the terms and conditions",()=>{
						var termsView = new TermsAndConditionsView(true);
						this.PresentViewController((UIViewController)termsView,true, null);
					})

				},
				new Section ("") {
					new StyledStringElement("Register",()=>{
						if(IsValid()){
							var user = new AppUser(){
								Name = FullNameEl.Value,
								Email = EmailEl.Value,
								username = UsernameEl.Value,
								password = PasswordEl.Value,
								passwordConfirmation = PasswordConfirmEl.Value
							};
							//TODO move validation to AppUser class and then push message back here for alert
							var args = new RegisterUserInvokerArgs(user);
							Register.Invoke(args);
						}
					}),
					new StyledStringElement ("Cancel", DismissView)
				},
			};
		}

		bool IsValid ()
		{
			string results = string.Empty;
			if(String.IsNullOrEmpty(FullNameEl.Value)){
				results += "Full Name is required." + Environment.NewLine;
			}

			if(String.IsNullOrEmpty(EmailEl.Value)){
				results += "Email is required." + Environment.NewLine;
			}else{
				//validate email
				if(!RegexUtilities.IsValidEmail(EmailEl.Value)){
					results += "Email format isn't recognized." + Environment.NewLine;
				}
			}

			if(String.IsNullOrEmpty(UsernameEl.Value)){
				results += "Username is required." + Environment.NewLine;
			}else{
				if(UsernameEl.Value.Length < 4){
					results += "Username should be longer than 4 characters." + Environment.NewLine;
				}

				if(UsernameEl.Value.IndexOf(" ") != -1){
					results += "Username should not have empty spaces." + Environment.NewLine;
				}
			}

			if(String.IsNullOrEmpty(PasswordEl.Value)){
				results += "Password is required." + Environment.NewLine;
			}

			if(String.IsNullOrEmpty(PasswordConfirmEl.Value)){
				results += "Password Confirmation is required." + Environment.NewLine;
			}

			if(PasswordEl.Value != PasswordConfirmEl.Value){
				results += "Passwords do not match." + Environment.NewLine;
			}

			if(!String.IsNullOrEmpty(results)){
				var alert = new UIAlertView ("Please Fix", results, null, "Okay");
				alert.Show ();
				return false;
			}else{
				return true;
			}
		}

		#region IRegistrationView implementation

//		public void ShowBusyIndicator ()
//		{
//			InvokeOnMainThread (() => {
//				this.ShowNetworkActivityIndicator ();
//				SubmitButton.Enabled = false;
//				SubmitButton.Alpha = .25f;
//			});
//
//		}
//
//		public void HideBusyIndicator ()
//		{
//			InvokeOnMainThread (() => {
//				this.HideNetworkActivityIndicator ();
//				SubmitButton.Enabled = true;
//				SubmitButton.Alpha = 1;
//			});
//		}

		public void DisplayError (string msg)
		{
			InvokeOnMainThread (() => {
				var alert = new UIAlertView("Oops", msg, null, "Okay");
				alert.Show();
			});
		}

		public void DismissView()
		{
			InvokeOnMainThread (() => this.DismissViewController (true, null));

		}

		public Invoker Register {
			get;
			private set;
		}

		public event EventHandler Dismissed;

		#endregion

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			DI.RequestMediator (this);

			AnalyticsUtil.TrackScreen (this.Title);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			DI.DestroyMediator (this);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			Dismissed(this, EventArgs.Empty);
		}
	}
}