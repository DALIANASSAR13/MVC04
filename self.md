## 3. What Is the Purpose of `async` and `await` Keywords in C#?

The `async` and `await` keywords are used in C# to support asynchronous programming.

- `async`: Allows a method to use the `await` keyword.
- `await`: Waits for an asynchronous operation to complete without blocking the current thread while the operation is in progress.

Benefits:

- Improves application scalability.
- Allows threads to handle other work while waiting for database operations.
- Makes asynchronous code easier to read and understand.

Conclusion: The `async` and `await` keywords help applications handle time-consuming operations more efficiently, especially when working with databases, files, and web requests.
