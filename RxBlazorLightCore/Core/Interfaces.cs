using System.Diagnostics.CodeAnalysis;
using R3;

// ReSharper disable once CheckNamespace -> use same namespace for interface
namespace RxBlazorLightCore
{
    /// <summary>
    /// Represents the reason for a change notification in the state management system.
    /// </summary>
    public enum ChangeReason
    {
        /// <summary>
        /// Represents a change that occurred due to an exception. This enum member is used to indicate that an
        /// exception has been encountered, and it serves as a reason
        /// </summary>
        EXCEPTION,

        /// <summary>
        /// Represents a change in the state of the service.
        /// This enum member is used to indicate that a change has occurred,
        /// which is not associated with an exception but rather with the state itself.
        /// </summary>
        STATE
    }

    /// <summary>
    /// A record struct representing the reason for a service change within state management.
    /// </summary>
    /// <remarks>
    /// This record struct is used to capture and convey the change reason within a service,
    /// associating an identifier with a specific reason for the change. This mechanism aids
    /// in the organization and tracking of state changes or exceptions as they relate to the
    /// specific services.
    /// </remarks>
    /// <param name="ID">
    /// A unique identifier associated with the specific instance of the service change.
    /// </param>
    /// <param name="Reason">
    /// The reason for the service change, which is based on the <see cref="ChangeReason"/> enumeration
    /// that includes various types like 'EXCEPTION' or 'STATE'.
    /// </param>
    public readonly record struct ServiceChangeReason(Guid ID, ChangeReason Reason);

    /// <summary>
    /// Represents an exception that occurs within a service, capturing the ID of the service context and the exception instance.
    /// </summary>
    /// <remarks>
    /// This struct is used to encapsulate details about exceptions that occur during the execution of a service operation.
    /// It includes a service-specific identifier and the associated exception object.
    /// </remarks>
    public readonly record struct ServiceException(Guid ID, Exception Exception);

    /// <summary>
    /// Represents the result of validating a state with a message and error status.
    /// </summary>
    /// <remarks>
    /// This is a readonly record struct used for conveying validation outcomes.
    /// It consists of a message describing the validation and a boolean indicating
    /// whether an error occurred.
    /// </remarks>
    public readonly record struct StateValidation(string Message, bool Error);

    /// <summary>
    /// Represents a scope of state management within the RxBlazorLight framework.
    /// </summary>
    /// <remarks>
    /// The IRxBLStateScope interface is designed for managing state scopes that ensure
    /// appropriate disposal and initialization of resources within the RxBlazorLight framework.
    /// Implementing classes will leverage these capabilities to perform actions once the context
    /// is ready and to ensure any necessary cleanup is performed.
    /// </remarks>
    public interface IRxBLStateScope : IDisposable
    {
        /// <summary>
        /// Method to perform actions when the context is ready for use. This method is intended
        /// to be executed as soon as the context has been initialized and is ready for any further
        /// action that depends on the initialization being complete. Implementations can override
        /// this method to define the specific actions to take once the context readiness is achieved.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation to signal that
        /// the context is ready and any necessary initialization tasks are complete.</returns>
        public ValueTask OnContextReadyAsync();
    }

    /// <summary>
    /// Defines a reactive business logic service in the RxBlazorLightCore framework.
    /// </summary>
    /// <remarks>
    /// The IRxBLService interface extends IDisposable and IStateInformation, adding functionality for working with reactive
    /// streams and state management. It exposes methods and properties for tracking service state changes, handling exceptions,
    /// and providing commands for state manipulation. The interface enables reactive programming patterns through observables
    /// and observers, facilitating asynchronous and state-driven operations.
    /// </remarks>
    public interface IRxBLService : IStateInformation, IDisposable
    {
        /// <summary>
        /// Gets an observable stream of <see cref="ServiceChangeReason"/> objects that represent reasons for changes
        /// occurring within the service. This observable can be used to subscribe to notifications of change events
        /// which are emitted as instances of <see cref="ServiceChangeReason"/>.
        /// </summary>
        public Observable<ServiceChangeReason> AsObservable { get; }

        /// <summary>
        /// Gets the observer which enables the service to react to emitted events or notifications.
        /// </summary>
        /// <remarks>
        /// This property provides access to the observer interface of the service, allowing it to observe and respond to events.
        /// Typically used within reactive programming paradigms.
        /// </remarks>
        public Observer<Unit> AsObserver { get; }

        /// <summary>
        /// Initiates any necessary actions when the service context is ready. This asynchronous operation
        /// should ensure that any required initialization is complete before further processing. It is invoked
        /// when the service is not yet initialized and is intended to make sure that the service is marked
        /// as initialized and that the state is updated accordingly.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the work of preparing the context and performing
        /// necessary setup actions once the context becomes ready.</returns>
        public ValueTask OnContextReadyAsync();

        /// <summary>
        /// Indicates whether the service has been initialized.
        /// </summary>
        public bool Initialized { get; }

        /// <summary>
        /// Gets a collection of <see cref="ServiceException"/> instances representing the exceptions encountered by the service.
        /// This property allows accessing information about errors that have occurred during the service's operation, each identified
        /// by a unique ID and the associated exception.
        /// </summary>
        public IEnumerable<ServiceException> Exceptions { get; }

        /// <summary>
        /// Clears the collection of exceptions that have been recorded. This method
        /// allows for resetting the state of stored exceptions, making it possible
        /// to start fresh, without any previously noted errors.
        /// </summary>
        public void ResetExceptions();

        /// <summary>
        /// Signals that the state has been updated and any components or services depending
        /// on the current state should be notified to refresh, recompute, or otherwise adjust
        /// to the new state. This method is typically called after changes to the state
        /// that require consumers to update their representation or behavior
        /// based on the altered state.
        /// </summary>
        public void StateHasChanged();

        /// <summary>
        /// Property that provides a synchronous command execution mechanism.
        /// </summary>
        /// <remarks>
        /// This property is part of the <see cref="IRxBLService"/> interface and allows the execution of commands
        /// encapsulated within an <see cref="IStateCommand"/>. It is typically used for responding to user actions
        /// or other state change triggers within the application. Implementations can define specific actions to be
        /// executed when the command is invoked.
        /// </remarks>
        /// <seealso cref="IRxBLService.CommandAsync"/>
        /// <seealso cref="IRxBLService.CancellableCommandAsync"/>
        public IStateCommand Command { get; }

        /// <summary>
        /// Represents an asynchronous command within the service, providing mechanisms for
        /// executing tasks asynchronously with support for state change notifications.
        /// </summary>
        /// <remarks>
        /// This property returns an instance of <see cref="IStateCommandAsync"/> that can be used to
        /// execute asynchronous operations in the context of the service. It is designed to support
        /// deferred state notifications and to identify the caller of state changes using a
        /// unique identifier.
        /// </remarks>
        public IStateCommandAsync CommandAsync { get; }

        /// <summary>
        /// Represents an asynchronous command that can be cancelled. This property provides
        /// functionality to execute asynchronous operations that support cancellation through
        /// a CancellationToken. It is part of the <see cref="IRxBLService"/> interface, allowing
        /// services to manage asynchronous state commands with the ability to respond to cancellation requests.
        /// </summary>
        public IStateCommandAsync CancellableCommandAsync { get; }
    }

    /// <summary>
    /// Indicates the current phase of the state management system.
    /// </summary>
    public enum StatePhase
    {
        /// <summary>
        /// Indicates that a state is transition process.
        /// </summary>
        CHANGING,

        /// <summary>
        /// Indicates that a state transition has been completed.
        /// </summary>
        CHANGED,

        /// <summary>
        /// Indicates that the changing of the state has been canceled before completion.
        /// </summary>
        CANCELED,

        /// <summary>
        /// Represents a phase in the state management system where an exception has occurred,
        /// interrupting the normal workflow or state transition process.
        /// </summary>
        EXCEPTION
    }

    /// <summary>
    /// Represents the contract for state information within the RxBlazorLightCore system.
    /// </summary>
    /// <remarks>
    /// This interface provides essential properties to describe the state and its attributes,
    /// including the phase of the state and its unique identifier. It also allows determining
    /// whether the state is disabled or operates independently.
    /// </remarks>
    public interface IStateInformation
    {
        /// <summary>
        /// Gets the current phase of the state management system, represented by a <see cref="StatePhase"/> enumeration.
        /// It indicates the current status of an operation within the system, which can be in one of several phases:
        /// changing, changed, canceled, or exception.
        /// </summary>
        public StatePhase Phase { get; }

        /// <summary>
        /// Gets the unique identifier (ID) for a particular instance of a state or service, represented as a <see cref="Guid"/>.
        /// This identifier is used to track and manage changes or exceptions within the system, providing a consistent reference
        /// across different components that implement the <see cref="IStateInformation"/> interface or belong to the <see cref="RxBLService"/>.
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// Indicates whether the current entity is disabled, typically due to being in a state of change
        /// or based on conditional logic defined by the implementing class. This property provides a
        /// mechanism to control the active state of UI components or services.
        /// </summary>
        public bool Disabled { get; }
    }

    /// <summary>
    /// Represents a state management interface for handling values of a specified type with functionality
    /// to get, set, and check the presence of the value.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being managed within the state.
    /// </typeparam>
    /// <remarks>
    /// Implementations of this interface are responsible for encapsulating a value and providing mechanisms to
    /// modify and query that value's existence. This is useful for scenarios where state changes need to be tracked
    /// or managed in a reactive programming model.
    /// </remarks>
    public interface IState<T> : IStateInformation
    {
        /// <summary>
        /// Gets or sets the current value of the state of type <typeparamref name="T"/>.
        /// This property is used to hold and manage the state value within implementations
        /// of the <see cref="IState{T}"/> interface, allowing the value to be retrieved and updated.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Sets the value of the state to the specified value. This method allows for
        /// updating the internal representation of the state with a new value.
        /// </summary>
        /// <param name="value">The new value to be assigned to the state.</param>
        public void SetValueSilent(T value);

        /// <summary>
        /// Determines whether the current state holds a non-null value. This method checks if
        /// the property <c>Value</c> has been assigned a non-null value, indicating that the state
        /// contains a meaningful data value.
        /// </summary>
        /// <returns><c>true</c> if the <c>Value</c> property is non-null; otherwise, <c>false</c>.</returns>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
    }

    /// <summary>
    /// Defines a contract for executing a state-related command within the state management system.
    /// </summary>
    /// <remarks>
    /// This interface extends from <see cref="IStateInformation"/> and provides a mechanism
    /// for executing actions tied to state commands.
    /// Implementors are expected to provide a concrete implementation of the Execute method.
    /// </remarks>
    public interface IStateCommand : IStateInformation
    {
        /// <summary>
        /// Executes a specified action, encapsulating functionality that
        /// changes the state of an object. This method is used to trigger
        /// the execution of state-altering operations defined within the
        /// provided delegate, allowing for the customization of the behavior
        /// as needed in different implementations.
        /// </summary>
        /// <param name="executeCallback">The delegate action to be executed, which performs
        /// the state-changing logic required by the calling context.</param>
        public void Execute(Action executeCallback);
    }

    /// <summary>
    /// An interface that provides asynchronous state command operations within the reactive state management system.
    /// </summary>
    /// <remarks>
    /// This interface is designed to define the base functionalities for asynchronous commands
    /// relating to state transitions or operations, enabling operations like notification of changes
    /// and cancellation of ongoing processes.
    /// </remarks>
    public interface IStateCommandBaseAsync
    {
        /// <summary>
        /// Notifies that a state change is about to occur. This method should be called
        /// prior to any modifications to the state, signaling to observers or handlers
        /// that a change is imminent and they may need to adjust behavior accordingly.
        /// </summary>
        public void NotifyChanging();

        /// <summary>
        /// Cancels the current operation if it is in progress. This method
        /// allows for manually stopping any ongoing task or operation associated
        /// with the implementing instance. It is typically invoked to ensure
        /// that the system can safely halt an operation that is no longer needed
        /// or when an interruption is required.
        /// </summary>
        public void Cancel();
    }

    /// <summary>
    /// Represents an asynchronous state command within the context of state management.
    /// </summary>
    /// <remarks>
    /// This interface extends both <see cref="IStateCommandBaseAsync"/> and <see cref="IStateCommand"/> to
    /// provide functionalities for executing state-related commands asynchronously with the support for cancellation.
    /// It aids in managing and executing tasks relevant to changes or operations in the application's state, allowing
    /// for deferred notifications and identification of the change caller.
    /// </remarks>
    public interface IStateCommandAsync : IStateCommandBaseAsync, IStateCommand
    {
        /// <summary>
        /// Indicates whether the command can be cancelled.
        /// This property allows the implementing class to determine
        /// if a cancellation operation is permissible for the current
        /// command execution context.
        /// </summary>
        public bool CanCancel { get; }

        /// <summary>
        /// Gets the unique identifier for the caller that initiated the current change process.
        /// This property can be used to determine the source of the command execution and to
        /// associate the execution with a specific change request within a state management process.
        /// </summary>
        public Guid? ChangeCallerID { get; }

        /// <summary>
        /// Provides a mechanism to propagate notifications that an operation should be canceled. Used to control the cancellation of tasks
        /// running asynchronously, allowing them to be canceled before they complete.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Executes an asynchronous operation defined by the <paramref name="executeCallbackAsync"/> function.
        /// Optionally, it allows for deferred notifications about the operation's progress or completion status.
        /// This method is intended to be used when asynchronous execution needs to be initiated on a state command.
        /// </summary>
        /// <param name="executeCallbackAsync">
        /// A function delegate that takes an <see cref="IStateCommandAsync"/> as a parameter and returns a <see cref="Task"/>
        /// representing the asynchronous operation to execute.
        /// </param>
        /// <param name="deferredNotification">
        /// A boolean parameter that determines whether notifications about the command's execution should
        /// be deferred. The default value is <c>false</c>, meaning immediate notification.
        /// If the value is <c>true</c>, the change notification must be invoked manually by calling <see cref="IStateCommandBaseAsync.NotifyChanging"/>
        /// </param>
        /// <param name="changeCallerID">
        /// An optional <see cref="Guid"/> parameter representing the identifier for the command execution's origin caller.
        /// It can be used to track or correlate changes.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous execution operation.</returns>
        public Task ExecuteAsync(Func<IStateCommandAsync, Task> executeCallbackAsync, bool deferredNotification = false, Guid? changeCallerID = null);
    }

    /// <summary>
    /// Represents an asynchronous observer interface for tracking progress state with a double precision value.
    /// </summary>
    /// <remarks>
    /// This interface is designed to observe and manage the progression of a task in an asynchronous operation,
    /// allowing the monitoring of progress values from 0 to 100.0, where -1.0 indicates an indeterminate state.
    /// It extends the <see cref="IStateCommandBaseAsync"/> and <see cref="IState{T}"/> interfaces, ensuring
    /// conformity to specific command execution patterns and state management protocols.
    /// </remarks>
    public interface IStateProgressObserverAsync : IStateCommandBaseAsync, IState<double>
    {
        /// <summary>
        /// Represents a special value used to signify an indeterminate state in progress operations.
        /// This value is typically used in asynchronous state progress observers to indicate that
        /// the progress is currently not measurable or not available.
        /// </summary>
        public const double InterminateValue = -1.0;

        /// <summary>
        /// Represents the minimum value for the state progress observer, used to denote the starting point or lowest progress in the observer's context.
        /// </summary>
        public const double MinValue = 0;

        /// <summary>
        /// Represents the maximum value for the progress, typically used to indicate completion in a progress observer.
        /// This constant is part of the <see cref="IStateProgressObserverAsync"/> interface and is set to 100.0.
        /// </summary>
        public const double MaxValue = 100.0;

        /// <summary>
        /// Provides an observer instance of type <see cref="Observer{T}"/> that monitors the state represented
        /// as a double precision floating point number. This observer facilitates subscription to observable
        /// sequences, enabling handling and processing of double-based state changes or progress updates.
        /// </summary>
        public Observer<double> AsObserver { get; }

        /// <summary>
        /// Gets the exception that has occurred during the execution of an asynchronous state progress operation.
        /// This property provides access to any errors that have been captured within the state observer,
        /// allowing for error handling and recovery processes to be implemented.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Resets the Exception property to null. This method is used to clear any existing
        /// exception data that has been recorded in the process of observing the state progress.
        /// It is typically invoked before starting a new operation to ensure that previous exception
        /// states do not interfere with new execution logic.
        /// </summary>
        public void ResetException();

        /// <summary>
        /// Executes an asynchronous operation using the provided callback function. This method clears any existing
        /// exceptions, initializes the observer core to handle potential errors, and sets the initial state value
        /// to indicate an indeterminate progress. The callback function is then invoked, which is expected to return
        /// a disposable resource to manage the operation's lifecycle.
        /// </summary>
        /// <param name="executeCallbackAsync">A callback function that takes the current <see cref="IStateProgressObserverAsync"/> instance
        /// as a parameter and returns an <see cref="IDisposable"/>. This function contains the logic for the operation to be executed asynchronously,
        /// allowing for state updates and error handling through the observer.</param>
        public void ExecuteAsync(Func<IStateProgressObserverAsync, IDisposable> executeCallbackAsync);
    }

    /// <summary>
    /// Represents the base functionality for managing state groups in a state management system.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value being managed by the state group.
    /// </typeparam>
    /// <remarks>
    /// This interface defines the essential operations and properties for state group management,
    /// including tracking a single state value, managing a collection of items,
    /// and determining whether a valid state value exists. It extends the <see cref="IStateInformation"/>
    /// interface, facilitating integration within a broader state management architecture.
    /// </remarks>
    public interface IStateGroupBase<T> : IStateInformation
    {
        /// <summary>
        /// Represents the current value of the state. The value is of type <typeparamref name="T"/> and can be nullable,
        /// indicating that no value may be presently assigned. This property is used to retrieve or update the state
        /// within a group of states, often for handling async state changes or multi-state components.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets the array of items of type <typeparamref name="T"/> contained within the state group. These items represent
        /// the elements or data that are part of the current state group and are accessible for operations and display purposes.
        /// </summary>
        public T[] Items { get; }

        /// <summary>
        /// Updates the current state with the provided value. This method is typically used to modify
        /// the state held within the implementing class, allowing for dynamic changes to state data.
        /// </summary>
        /// <param name="value">The new value to update the state with.</param>
        public void Update(T value);

        /// <summary>
        /// Determines whether the underlying value is present and not null. This method checks
        /// if the object of type <typeparamref name="T"/> has been set or initialized to a
        /// non-null value.
        /// </summary>
        /// <returns>A boolean value indicating whether the <typeparamref name="T"/> value is not null.</returns>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue();
    }

    /// <summary>
    /// Represents a group of stateful values, providing mechanisms to change and manage
    /// these values within a reactive UI framework.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the items contained within the state group. This allows the group to
    /// manage state for complex and basic data types as dictated by the application requirements.
    /// </typeparam>
    /// <remarks>
    /// The IStateGroup interface extends the functionalities of the base state group to
    /// include value-changing logic, allowing for optional callback invocation when a
    /// value transition occurs. This interface is crucial in scenarios where state transition
    /// needs to be tracked or handled differently based on changing states.
    /// </remarks>
    public interface IStateGroup<T> : IStateGroupBase<T>
    {
        /// <summary>
        /// Changes the current value of the state group to the specified new value.
        /// If provided, a callback function is executed before the value changes,
        /// allowing for customized actions to occur during the value transition.
        /// </summary>
        /// <param name="value">The new value to be set for the state group.</param>
        /// <param name="changingCallback">An optional callback function that takes
        /// the old value and the new value, invoked before the value is changed. This
        /// can be used to implement pre-change logic or validations.</param>
        public void ChangeValue(T value, Action<T, T>? changingCallback = null);
    }

    /// <summary>
    /// Asynchronous state management interface for handling state groups.
    /// </summary>
    /// <typeparam name="T">
    /// Represents the type of the state value managed by this group.
    /// </typeparam>
    /// <remarks>
    /// This interface extends <see cref="IStateGroupBase{T}"/> to provide asynchronous
    /// operations for changing state values within a group. It is designed to support
    /// scenarios where state changes might involve asynchronous operations or callbacks.
    /// </remarks>
    public interface IStateGroupAsync<T> : IStateGroupBase<T>
    {
        /// <summary>
        /// Asynchronously changes the current value within the state group. This method allows the caller
        /// to specify a new value and optionally execute a callback function asynchronously during the changing
        /// process, providing an opportunity to implement actions such as validation or logging.
        /// </summary>
        /// <param name="value">The new value to be set within the state group.</param>
        /// <param name="changingCallbackAsync">An optional asynchronous callback function that is invoked during the changing process,
        /// potentially to handle actions required before finalizing the value change.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation of changing the value. Completion of the task
        /// signifies that the change has been processed and any provided callback function has been executed.</returns>
        public Task ChangeValueAsync(T value, Func<T, T, Task>? changingCallbackAsync = null);
    }
}
