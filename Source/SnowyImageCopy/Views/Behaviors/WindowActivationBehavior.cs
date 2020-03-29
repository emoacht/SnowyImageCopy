using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

using Expression = System.Linq.Expressions.Expression;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Activates attached Window.
	/// </summary>
	[TypeConstraint(typeof(Window))]
	public class WindowActivationBehavior : Behavior<Window>
	{
		#region Property

		/// <summary>
		/// Sender of an event to be listened
		/// </summary>
		public object SenderObject
		{
			get { return (object)GetValue(SenderObjectProperty); }
			set { SetValue(SenderObjectProperty, value); }
		}
		public static readonly DependencyProperty SenderObjectProperty =
			DependencyProperty.Register(
				"SenderObject",
				typeof(object),
				typeof(WindowActivationBehavior),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Name of an event to be listened
		/// </summary>
		public string EventName
		{
			get { return (string)GetValue(EventNameProperty); }
			set { SetValue(EventNameProperty, value); }
		}
		public static readonly DependencyProperty EventNameProperty =
			DependencyProperty.Register(
				"EventName",
				typeof(string),
				typeof(WindowActivationBehavior),
				new FrameworkPropertyMetadata(null));

		#endregion

		protected override void OnAttached()
		{
			base.OnAttached();

			AddHandler();
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			RemoveHandler();
		}

		private EventInfo _eventInfo;
		private Delegate _handler;

		private void AddHandler()
		{
			if ((SenderObject is null) || string.IsNullOrEmpty(EventName))
				return;

			_eventInfo = SenderObject.GetType().GetEvent(EventName);
			if (_eventInfo is null)
				return;

			_handler = CreateDelegate(_eventInfo, Activate);
			_eventInfo.AddEventHandler(SenderObject, _handler);
		}

		private void RemoveHandler()
		{
			if ((SenderObject is null) || (_eventInfo is null))
				return;

			_eventInfo.RemoveEventHandler(SenderObject, _handler);
		}

		private static Delegate CreateDelegate(EventInfo eventInfo, Action action)
		{
			var handlerType = eventInfo.EventHandlerType;
			var parameters = handlerType.GetMethod("Invoke").GetParameters()
				.Select(x => Expression.Parameter(x.ParameterType, x.Name))
				.ToArray();

			var body = Expression.Call(Expression.Constant(action), action.GetType().GetMethod("Invoke"));
			var lambda = Expression.Lambda(body, parameters);

			return Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);
		}

		private void Activate()
		{
			var window = this.AssociatedObject;
			if (window.IsActive)
				return;

			if (window.WindowState == WindowState.Minimized)
				window.WindowState = WindowState.Normal;

			window.Activate();
		}
	}
}