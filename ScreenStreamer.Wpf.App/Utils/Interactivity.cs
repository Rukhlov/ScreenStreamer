﻿using Microsoft.Xaml.Behaviors;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Prism.Interactivity
{
    /// <summary>
    /// Base behavior to handle connecting a <see cref="System.Windows.Controls.Control"/> to a Command.
    /// </summary>
    /// <typeparam name="T">The target object must derive from Control.</typeparam>
    /// <remarks>
    /// CommandBehaviorBase can be used to provide new behaviors for commands.
    /// </remarks>
    public class CommandBehaviorBase<T> where T : UIElement
    {
        private ICommand _command;
        private object _commandParameter;
        private readonly WeakReference _targetObject;
        private readonly EventHandler _commandCanExecuteChangedHandler;

        /// <summary>
        /// Constructor specifying the target object.
        /// </summary>
        /// <param name="targetObject">The target object the behavior is attached to.</param>
        public CommandBehaviorBase(T targetObject)
        {
            _targetObject = new WeakReference(targetObject);

            _commandCanExecuteChangedHandler = CommandCanExecuteChanged;
        }

        bool _autoEnabled = true;
        /// <summary>
        /// If <c>true</c> the target object's IsEnabled property will update based on the commands ability to execute.
        /// If <c>false</c> the target object's IsEnabled property will not update.
        /// </summary>
        public bool AutoEnable
        {
            get { return _autoEnabled; }
            set
            {
                _autoEnabled = value;
                UpdateEnabledState();
            }
        }

        /// <summary>
        /// Corresponding command to be execute and monitored for <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        public ICommand Command
        {
            get { return _command; }
            set
            {
                if (_command != null)
                {
                    _command.CanExecuteChanged -= _commandCanExecuteChangedHandler;
                }

                _command = value;
                if (_command != null)
                {
                    _command.CanExecuteChanged += _commandCanExecuteChangedHandler;
                    UpdateEnabledState();
                }
            }
        }

        /// <summary>
        /// The parameter to supply the command during execution.
        /// </summary>
        public object CommandParameter
        {
            get { return _commandParameter; }
            set
            {
                if (_commandParameter != value)
                {
                    _commandParameter = value;
                    UpdateEnabledState();
                }
            }
        }

        /// <summary>
        /// Object to which this behavior is attached.
        /// </summary>
        protected T TargetObject
        {
            get
            {
                return _targetObject.Target as T;
            }
        }


        /// <summary>
        /// Updates the target object's IsEnabled property based on the commands ability to execute.
        /// </summary>
        protected virtual void UpdateEnabledState()
        {
            if (TargetObject == null)
            {
                Command = null;
                CommandParameter = null;
            }
            else if (Command != null)
            {

                if (AutoEnable)
                    TargetObject.IsEnabled = Command.CanExecute(CommandParameter);

            }
        }

        private void CommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateEnabledState();
        }

        /// <summary>
        /// Executes the command, if it's set, providing the <see cref="CommandParameter"/>.
        /// </summary>
        protected virtual void ExecuteCommand(object parameter)
        {
            if (Command != null)
                Command.Execute(CommandParameter ?? parameter);
        }
    }

    /// <summary>
    /// Trigger action that executes a command when invoked. 
    /// It also maintains the Enabled state of the target control based on the CanExecute method of the command.
    /// </summary>
    public class InvokeCommandAction : TriggerAction<UIElement>
    {
        private ExecutableCommandBehavior _commandBehavior;

        /// <summary>
        /// Dependency property identifying if the associated element should automaticlaly be enabled or disabled based on the result of the Command's CanExecute
        /// </summary>
        public static readonly DependencyProperty AutoEnableProperty =
            DependencyProperty.Register("AutoEnable", typeof(bool), typeof(InvokeCommandAction),
                new PropertyMetadata(true, (d, e) => ((InvokeCommandAction)d).OnAllowDisableChanged((bool)e.NewValue)));

        /// <summary>
        /// Gets or sets whther or not the associated element will automatically be enabled or disabled based on the result of the commands CanExecute
        /// </summary>
        public bool AutoEnable
        {
            get { return (bool)GetValue(AutoEnableProperty); }
            set { SetValue(AutoEnableProperty, value); }
        }

        private void OnAllowDisableChanged(bool newValue)
        {
            var behavior = GetOrCreateBehavior();
            if (behavior != null)
                behavior.AutoEnable = newValue;
        }

        /// <summary>
        /// Dependency property identifying the command to execute when invoked.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(InvokeCommandAction),
                new PropertyMetadata(null, (d, e) => ((InvokeCommandAction)d).OnCommandChanged((ICommand)e.NewValue)));

        /// <summary>
        /// Gets or sets the command to execute when invoked.
        /// </summary>
        public ICommand Command
        {
            get { return GetValue(CommandProperty) as ICommand; }
            set { SetValue(CommandProperty, value); }
        }

        private void OnCommandChanged(ICommand newValue)
        {
            var behavior = GetOrCreateBehavior();
            if (behavior != null)
                behavior.Command = newValue;
        }

        /// <summary>
        /// Dependency property identifying the command parameter to supply on command execution.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(InvokeCommandAction),
                new PropertyMetadata(null, (d, e) => ((InvokeCommandAction)d).OnCommandParameterChanged(e.NewValue)));

        /// <summary>
        /// Gets or sets the command parameter to supply on command execution.
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        private void OnCommandParameterChanged(object newValue)
        {
            var behavior = GetOrCreateBehavior();
            if (behavior != null)
                behavior.CommandParameter = newValue;
        }

        /// <summary>
        /// Dependency property identifying the TriggerParameterPath to be parsed to identify the child property of the trigger parameter to be used as the command parameter.
        /// </summary>
        public static readonly DependencyProperty TriggerParameterPathProperty =
            DependencyProperty.Register("TriggerParameterPath", typeof(string), typeof(InvokeCommandAction),
                new PropertyMetadata(null, (d, e) => { }));

        /// <summary>
        /// Gets or sets the TriggerParameterPath value.
        /// </summary>
        public string TriggerParameterPath
        {
            get { return GetValue(TriggerParameterPathProperty) as string; }
            set { SetValue(TriggerParameterPathProperty, value); }
        }

        /// <summary>
        /// Public wrapper of the Invoke method.
        /// </summary>
        public void InvokeAction(object parameter)
        {
            Invoke(parameter);
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="parameter">This parameter is passed to the command; the CommandParameter specified in the CommandParameterProperty is used for command invocation if not null.</param>
        protected override void Invoke(object parameter)
        {
            if (!string.IsNullOrEmpty(TriggerParameterPath))
            {
                //Walk the ParameterPath for nested properties.
                var propertyPathParts = TriggerParameterPath.Split('.');
                object propertyValue = parameter;
                foreach (var propertyPathPart in propertyPathParts)
                {
                    var propInfo = propertyValue.GetType().GetTypeInfo().GetProperty(propertyPathPart);
                    propertyValue = propInfo.GetValue(propertyValue);
                }
                parameter = propertyValue;
            }

            var behavior = GetOrCreateBehavior();

            if (behavior != null)
            {
                behavior.ExecuteCommand(parameter);
            }
        }

        /// <summary>
        /// Sets the Command and CommandParameter properties to null.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            Command = null;
            CommandParameter = null;

            _commandBehavior = null;
        }

        /// <summary>
        /// This method is called after the behavior is attached.
        /// It updates the command behavior's Command and CommandParameter properties if necessary.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            // In case this action is attached to a target object after the Command and/or CommandParameter properties are set,
            // the command behavior would be created without a value for these properties.
            // To cover this scenario, the Command and CommandParameter properties of the behavior are updated here.
            var behavior = GetOrCreateBehavior();

            behavior.AutoEnable = AutoEnable;

            if (behavior.Command != Command)
                behavior.Command = Command;

            if (behavior.CommandParameter != CommandParameter)
                behavior.CommandParameter = CommandParameter;
        }

        private ExecutableCommandBehavior GetOrCreateBehavior()
        {
            // In case this method is called prior to this action being attached, 
            // the CommandBehavior would always keep a null target object (which isn't changeable afterwards).
            // Therefore, in that case the behavior shouldn't be created and this method should return null.
            if (_commandBehavior == null && AssociatedObject != null)
            {
                _commandBehavior = new ExecutableCommandBehavior(AssociatedObject);
            }

            return _commandBehavior;
        }

        /// <summary>
        /// A CommandBehavior that exposes a public ExecuteCommand method. It provides the functionality to invoke commands and update Enabled state of the target control.
        /// It is not possible to make the <see cref="InvokeCommandAction"/> inherit from <see cref="CommandBehaviorBase{T}"/>, since the <see cref="InvokeCommandAction"/>
        /// must already inherit from <see cref="TriggerAction{T}"/>, so we chose to follow the aggregation approach.
        /// </summary>
        private class ExecutableCommandBehavior : CommandBehaviorBase<UIElement>
        {
            /// <summary>
            /// Constructor specifying the target object.
            /// </summary>
            /// <param name="target">The target object the behavior is attached to.</param>
            public ExecutableCommandBehavior(UIElement target)
                : base(target)
            {
            }

            /// <summary>
            /// Executes the command, if it's set.
            /// </summary>
            public new void ExecuteCommand(object parameter)
            {
                base.ExecuteCommand(parameter);
            }
        }
    }
}