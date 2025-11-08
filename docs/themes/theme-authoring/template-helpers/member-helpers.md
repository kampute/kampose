# Member Helpers

Member helpers work with .NET types, members, and references. These helpers provide language-specific rendering for code elements and provide proper URLs cross-referencing documentation pages.

| Helper                                  | Purpose                                                                            | Parameters | Returns   |
|-----------------------------------------|------------------------------------------------------------------------------------|------------|-----------|
| [`memberDefinition`](#memberdefinition) | Renders definition of a type or type member according to language syntax           | `element`  | N/A       |
| [`memberSignature`](#membersignature)   | Renders signature of a namespace, type or type member according to language syntax | `element`  | N/A       |
| [`memberName`](#membername)             | Returns name of a namespace, type or type member, including its declaring type     | `element`  | `string`  |
| [`memberUrl`](#memberurl)               | Returns URL to documentation page of a namespace, type, or type member             | `element`  | `uri`     |
| [`memberCategory`](#membercategory)     | Returns category of a namespace, type, or type member                              | `element`  | `string`  |


## `memberDefinition`

Renders a complete definition for a code element directly to the output.

The definition includes attributes, access modifiers, return type, name, parameters, and constraints (if any) of the member formatted according to the language syntax.

**Syntax:**
```hbs
{{#memberDefinition element}}
```

**Parameters:**
- `element` (object) - A documentation model, metadata object, of code reference string that represents a type or type member

**Examples:**
```hbs
{{#memberDefinition model}}
{{#memberDefinition 'M:System.DateTime.ToString(System.String)'}}  {{!-- public string ToString(string format) --}}
```

## `memberSignature`

Renders a formatted signature for a code element directly to the output.

The signature includes name and parameters (if any) of the code element formatted according to the language syntax.

**Syntax:**
```hbs
{{#memberSignature element}}
```

**Parameters:**
- `element` (object) - A documentation model, metadata object, or code reference string that represents a namespace, type, or type member

**Examples:**
```hbs
{{#memberSignature model}}
{{#memberSignature 'M:System.DateTime.ToString(System.String)'}}  {{!-- ToString(string) --}}
```

## `memberName`

Returns the display name of a code element, including its declaring type if applicable.

The name is formatted according to the language syntax.

**Syntax:**
```hbs
{{#memberName element}}
```

**Parameters:**
- `element` (object) - A documentation model, metadata object, or code reference string that represents a namespace, type, or type member

**Returns:**
Display name string

**Examples:**
```hbs
{{#memberName model}}
{{#memberName 'M:System.DateTime.ToString(System.String)'}}  {{!-- DateTime.ToString --}}
```

## `memberUrl`

Returns the URL to the documentation page of a code element.

Based on the context, the returned URL can be either relative or absolute. In case of a relative URL, it is relative to the current documentation page.

**Syntax:**
```hbs
{{#memberUrl element}}
```

**Parameters:**
- `element` (object) - A documentation model, metadata object, or code reference string that represents a namespace, type, or type member

**Returns:**
URI object representing the URL to the documentation page

**Examples:**
```hbs
{{#memberUrl model}}
{{#memberUrl 'M:System.DateTime.ToString(System.String)'}}  {{!-- https://learn.microsoft.com/en-us/dotnet/api/system.datetime.tostring#system-datetime-tostring(system-string) --}}
```

## `memberCategory`

Determines the category of a code element

**Syntax:**
```hbs
{{#memberCategory element}}
```

**Parameters:**
- `element` (object) - A documentation model, metadata object, or code reference string that represents a namespace, type, or type member

**Returns:**
A string representing the category of the member, such as:
- `Namespace`
- `Class`
- `Interface`
- `Struct`
- `Enum`
- `Delegate`
- `Constructor`
- `Field`
- `Method`
- `Property`
- `Event`
- `Operator`

**Examples:**
```hbs
{{#memberCategory model}}
{{#memberCategory 'M:System.DateTime.ToString(System.String)'}}  {{!-- Method }}
```
