namespace SurveyBasket.Api.Authentication.Filters;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}





//Class: PermissionRequirement
//- This is a class that represents a requirement for authorization.
//- It is used to check if a user has a specific permission to access a resource or perform an action.

//Constructor: `PermissionRequirement(string permission)
//- A constructor is a special method that is called when an object of the class is created.
//- This constructor takes a `string` parameter called `permission`. This parameter is used to define the specific permission required (e.g., "ReadProducts", "WriteProducts").

// IAuthorizationRequirement
//- This means the class implements the `IAuthorizationRequirement` interface, which is a built-in part of ASP.NET Core.
//- The `IAuthorizationRequirement` interface is a marker interface. It doesn’t define any methods but tells ASP.NET Core that this class represents a rule that needs to be checked during authorization.

//public string Permission { get; } = permission;
//- This line creates a property called `Permission`.
//- The property is read-only (`get;`) and gets its value from the `permission` parameter passed into the constructor.
//- In simple terms, it stores the permission name that you want to check later.

//---

//What Does It Do?
//1. The `PermissionRequirement` class is like a small box that holds a single permission name
//2. When you create an instance of this class, you pass in the permission name(e.g., "Permission.ReadProducts").
//3. Later, when your app checks if a user is authorized, this class will be used by an ** authorization handler** to compare the user's permissions against the required one.

//---

//### **Example in Action**
//Let’s say your app has a feature where only users with the "ReadProducts" permission can view a list of products.

//1. You create a `PermissionRequirement` for "ReadProducts":
//   ```csharp
//   var requirement = new PermissionRequirement("Permission.ReadProducts");
//   ```

//2. The app’s authorization system uses this requirement to check if the user has the required permission.

//3. If the user has "Permission.ReadProducts" in their list of permissions, they are allowed to access the feature.

//4. If not, they are denied access.



//Why Use This?
//The `PermissionRequirement` class is part of a flexible system for controlling what actions users can perform in your application.By defining permissions in this way, you can:
//- Create reusable rules for authorization.
//- Add, modify, or remove permissions without hardcoding them everywhere in your app.
//- Make your code more readable and easier to maintain.

//---

