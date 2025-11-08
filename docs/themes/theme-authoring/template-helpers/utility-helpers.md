# Utility Helpers

General utility functions for common operations including value selection, numeric manipulation, date formatting, and content processing.

| Helper                        | Purpose                                                                            | Parameters                            | Returns   |
|-------------------------------|------------------------------------------------------------------------------------|---------------------------------------|-----------|
| [`isUndefined`](#isUndefined) | Checks if a value is undefined                                                     | `value`                               | `boolean` |
| [`isNull`](#isNull)           | Checks if a value is null                                                          | `value`                               | `boolean` |
| [`isOdd`](#isOdd)             | Checks if an integer number is odd                                                 | `value`                               | `boolean` |
| [`select`](#select)           | Selects a value from choices based on a selector                                   | `selector`, `choice1`, `choice2`, ... | `any`     |
| [`len`](#len)                 | Returns the length of a string or array                                            | `value`                               | `number`  |
| [`now`](#now)                 | Returns current date/time with formatting                                          | `format?`                             | `string`  |
| [`json`](#json)               | Converts value to JSON string representation                                       | `value`                               | `string`  |
| [`literal`](#literal)         | Renders a value as a formatted literal constant in the target programming language | `value`                               | N/A       |
| [`cref`](#cref)               | Renders a link to the documentation of a code reference                            | `cref`                                | N/A       |
| [`markdown`](#markdown)       | Renders block content in Markdown as the current layout output                     | (block helper)                        | N/A       |
| [`stripTags`](#striptags)     | Removes HTML tags from block content                                               | (block helper)                        | N/A       |

## `isUndefined`

Checks if a value is undefined in the current template context.

This is useful do distinguish between a variable that has not been set and one that set to `null` or `false`.

**Syntax:**
```hbs
{{#isUndefined value}}
```

**Parameters:**
- `value` (any) - The value to check

**Returns:**
`true` if the value is undefined, `false` otherwise.

**Examples:**
```hbs
{{#if (isUndefined model.declaredMembers)}}
  The model is not capable of declaring members.
{{/if}}
```

## `isNull`

Checks if a value is explicitly set to null.

**Syntax:**
```hbs
{{#isNull value}}
```

**Parameters:**
- `value` (any) - The value to check

**Returns:**
`true` if the value is null, `false` otherwise.

**Examples:**
```hbs
{{#if (isNull model.doc.threadSafety.isStaticSafe)}}
  No thread safety information for static members
{{/if}}
```

## `isOdd`

Checks if an integer number is odd.

**Syntax:**
```hbs
{{#isOdd value}}
```

**Parameters:**
- `value` (number) - The value to check

**Returns:**
`true` if the value is odd, `false` otherwise.

**Examples:**
```hbs
{{#isOdd 5}}                      {{!-- true --}}
{{#isOdd 10}}                     {{!-- false --}}
{{#if (isOdd @index)}}Odd{{/if}}  {{!-- Displays "Odd" if current index is odd --}}
```

## `select`

Selects a value from a list of choices based on a selector value. The selection behavior depends on the number of choices provided:

- **One choice**: Returns the single choice regardless of the selector
- **Two choices**: Returns the first choice if the selector is truthy, otherwise the second choice
- **More than two choices**: Converts the selector to an integer and uses it as a zero-based index into the choices. Returns `null` if conversion fails or index is out of range.

**Syntax:**
```hbs
{{#select selector choices}}
{{#select selector choice1 choice2 ...}}
```

**Parameters:**
- `selector` (any) - The value used to determine which choice to select
- `choices` (array) - collection of choice values
- `choice1`, `choice2`, ... (any) - Individual choice values

**Returns:**
The selected value based on the selector and number of choices, or `null` if no valid selection can be made.

**Examples:**
```hbs
{{#select model.metadata.isStatic "Static" "Instance"}}
{{!-- Returns "Static" if model is static, otherwise "Instance" --}}

{{#select @index "first" "second" "third"}}
{{!-- Returns "first" if @index is 0, "second" if 1, "third" if 2, null otherwise --}}

{{#select item.status ["pending", "approved", "rejected"]}}
{{!-- If item.status is 0, returns "pending"; if 1, "approved"; if 2, "rejected" --}}

{{#select 1 "A" "B" "C" "D"}}
{{!-- Returns "B" (index 1) --}}
```

## `len`

Returns the length of a string or array.

**Syntax:**
```hbs
{{#len value}}
```

**Parameters:**
- `value` (string or array) - The string or array to measure

**Returns:**
The length of the string or array as a number.

**Examples:**
```hbs
{{#len "Hello World"}}           {{!-- 11 --}}
{{#len [2, 3, 5]}}               {{!-- 3 --}}
{{#len model.name}}              {{!-- Length of model name string --}}
{{#len model.directInterfaces}}  {{!-- Length of properties collection --}}
```

## `now`

Returns the string representation of the current date and time.

The optional `format` parameter allows specifying a .NET DateTime format string to control the output format.

The culture used for formatting is the invariant culture.

**Syntax:**
```hbs
{{#now}}
{{#now format}}
```

**Parameters:**
- `format` (string, optional) - .NET DateTime format string

**Returns:**
Current date/time as string

**Examples:**
```hbs
{{#now}}                       {{!-- "1/15/2024 2:30:45 PM" --}}
{{#now "yyyy-MM-dd"}}          {{!-- "2024-01-15" --}}
{{#now "HH:mm:ss"}}            {{!-- "14:30:45" --}}
{{#now "MMM dd, yyyy"}}        {{!-- "Jan 15, 2024" --}}
```

## `json`

Converts an object to JSON string representation.

Circular references in the object graph are automatically handled by replacing cyclic references with `null` in the JSON output. This means a property may appear as `null` in the JSON string even though the actual object is not null—it's simply part of a cycle that cannot be fully serialized.

**Syntax:**
```hbs
{{#json value}}
```

**Parameters:**
- `value` (any) - The value to convert to JSON

**Returns:**
A JSON string representation of the value or null if the value is undefined.

**Examples:**
```hbs
<body data-model="{{#json model}}">
```

## `literal`

Renders a value as a formatted literal constant in the target programming language specified by the current documentation context.

**Syntax:**
```hbs
{{#literal value}}
```

**Parameters:**
- `value` (any) - The value to format as a literal

**Returns:**
No return value; renders the resulting content directly.

**Examples:**
```hbs
{{#literal null}}           {{!-- null --}}
{{#literal true}}           {{!-- true --}}
{{#literal false}}          {{!-- false --}}
{{#literal 42}}             {{!-- 42 --}}
{{#literal -123.45}}        {{!-- -123.45 --}}
{{#literal '\n'}}           {{!-- '\\n' --}}
{{#literal "Hello World"}}  {{!-- "&quot;Hello World&quot;" (HTML escaped) --}}
```

## `cref`
Renders a link to the documentation of a code reference.

**Syntax:**
```hbs
{{#cref cref}}
```

**Parameters:**
- `cref` (string) - The code reference identifier

**Returns:**
No return value; renders the resulting link directly.

**Examples:**
```hbs
See also {{#cref "M:System.DateTime.ToString(System.String)"}} for more details.
{{!-- For HTML output, renders: See also <a href="https://learn.microsoft.com/en-us/dotnet/api/system.datetime.tostring#system-datetime-tostring(system-string)" rel="code-reference">DateTime.ToString(string)</a> for more details. --}}
{{!-- For Markdown output, renders: See also [DateTime.ToString(string)](https://learn.microsoft.com/en-us/dotnet/api/system.datetime.tostring#system-datetime-tostring(system-string)) for more details. --}}
```

## `markdown`

Renders block content written in Markdown and converts it to the current documentation layout's output.

**Syntax:**
```hbs
{{#markdown}}...markdown-content...{{/markdown}}
```

**Parameters:**
No parameters.

**Returns:**
No return value; renders the resulting content directly.

**Examples:**
```hbs
{{#markdown}}
This is a paragraph with **bold** text.
{{/markdown}}
{{!-- For HTML output, renders: "<p>This is a paragraph with <strong>bold</strong> text.</p> --}}
{{!-- For Markdown output, renders: "This is a paragraph with **bold** text. --}}
```

```hbs
{{#if assembly.metadata.attributes.releaseNotes}}
<div class="release-notes">
    {{#markdown}}{{{assembly.metadata.attributes.releaseNotes}}}{{/markdown}}
</div>
{{/if}}
{{!-- Renders release notes, converting it to HTML if needed. --}}
{{!-- To avoid double escaping of Markdown content, use triple braces. --}}
```

## `stripTags`

Removes HTML tags from block content and renders the cleaned text.

**Syntax:**
```hbs
{{#stripTags}}...html-content...{{/stripTags}}
```

**Parameters:**
No parameters.

**Returns:**
No return value; renders the resulting content directly.

**Examples:**
```hbs
{{#stripTags}}<p>Hello <strong>World</strong></p>{{/stripTags}}
{{!-- Renders: "Hello World" --}}
```

```hbs
<meta name="description" content="{{#stripTags}}{{model.doc.summary}}{{/stripTags}}" />
{{!-- Renders plain text of model's summary line into meta tag --}}
```
