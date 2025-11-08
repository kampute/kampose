# String Helpers

String manipulation and formatting functions that transform text values and return processed strings.

| Helper                            | Purpose                                                                | Parameters           | Returns    |
|-----------------------------------|------------------------------------------------------------------------|----------------------|------------|
| [`format`](#format)               | Converts a value to a string with optional formatting                  | `value`, `format?`   | `string`   |
| [`upperCase`](#uppercase)         | Converts a string to uppercase                                         | `text`               | `string`   |
| [`lowerCase`](#lowercase)         | Converts a string to lowercase                                         | `text`               | `string`   |
| [`kebabCase`](#kebabcase)         | Converts a string to kebab-case                                        | `text`               | `string`   |
| [`snakeCase`](#snakecase)         | Converts a string to snake_case                                        | `text`               | `string`   |
| [`titleCase`](#titlecase)         | Converts a string to title case                                        | `text`               | `string`   |
| [`split`](#split)                 | Splits a string by a separator                                         | `input`, `separator` | `string[]` |
| [`concat`](#concat)               | Concatenates string representation of some values into a single string | `...values`          | `string`   |
| [`firstNonBlank`](#firstnonblank) | Returns the first non-blank value                                      | `...values`          | `string?`  |

## `format`

Returns the string representation of an object with optional formatting.

The `format` parameter can be used to specify a format string for formattable objects like numbers and dates.

The culture used for formatting is the invariant culture.

**Syntax:**
```hbs
{{#format value}}
{{#format value format}}
```

**Parameters:**
- `value` (any) - The object to convert to string
- `format` (string, optional) - Format string for formattable objects

**Returns:**
String representation of the object

**Examples:**
```hbs
{{#format 42}}                    {{!-- "42" --}}
{{#format 3.14159 "F2"}}          {{!-- "3.14" --}}
{{#format true}}                  {{!-- "True" --}}
{{#format null}}                  {{!-- "" --}}
{{#format topic}}                 {{!-- String value of topic, which is its title --}}
```

## `split`

Splits a string by a specified delimiter.

**Syntax:**
```hbs
{{#split string delimiter}}
```

**Parameters:**
- `string` (string) - The string to split
- `delimiter` (string) - The delimiter to split by

**Returns:**
Array of string parts

**Examples:**
```hbs
<ul>
  {{#each (split "apple,banana,cherry" ",")}}
  <li>{{this}}</li>
  {{/each}}
</ul>

{{#each (split model.name ".")}}
  {{this}}{{#unless @last}}&bull;{{/unless}}
{{/each}}
```

## `concat`

Concatenates string representation of some values into a single string.

**Syntax:**
```hbs
{{#concat values}}
{{#concat value1 value2 value3 ...}}
```

**Parameters:**
- `values` (array) - Collection of values to concatenate
- `value1, value2, value3 ...` (any) - values to concatenate

**Returns:**
Concatenated string

**Examples:**
```hbs
{{#concat "Hello" " " "World"}}             {{!-- "Hello World" --}}
{{#concat model.namespace "." model.name}}  {{!-- "MyApp.MyClass" --}}
{{#concat ["a", "b"]}}                      {{!-- "ab" --}}
```

## `upperCase`

Converts a string to uppercase.

**Syntax:**
```hbs
{{#upperCase string}}
```

**Parameters:**
- `string` (string) - The string to convert

**Returns:**
Uppercase string

**Examples:**
```hbs
{{#upperCase "hello world"}}           {{!-- "HELLO WORLD" --}}
{{#upperCase model.name}}              {{!-- "MYCLASS" --}}
```

## `lowerCase`

Converts a string to lowercase.

**Syntax:**
```hbs
{{#lowerCase string}}
```

**Parameters:**
- `string` (string) - The string to convert

**Returns:**
Lowercase string

**Examples:**
```hbs
{{#lowerCase "HELLO WORLD"}}           {{!-- "hello world" --}}
{{#lowerCase model.name}}              {{!-- "myclass" --}}
```

## `kebabCase`

Converts a string to kebab-case (lowercase with hyphens).

**Syntax:**
```hbs
{{#kebabCase string}}
```

**Parameters:**
- `string` (string) - The string to convert

**Returns:**
Kebab-case string

**Examples:**
```hbs
{{#kebabCase "Hello World"}}           {{!-- "hello-world" --}}
{{#kebabCase "XMLParser"}}             {{!-- "xml-parser" --}}
{{#kebabCase model.name}}              {{!-- "my-class" --}}
```

## `snakeCase`

Converts a string to snake_case (lowercase with underscores).

**Syntax:**
```hbs
{{#snakeCase string}}
```

**Parameters:**
- `string` (string) - The string to convert

**Returns:** Snake_case string

**Examples:**
```hbs
{{#snakeCase "Hello World"}}           {{!-- "hello_world" --}}
{{#snakeCase "XMLParser"}}             {{!-- "xml_parser" --}}
{{#snakeCase model.name}}              {{!-- "my_class" --}}
```

## `titleCase`

Converts a string to title case (first letter of each word capitalized).

**Syntax:**
```hbs
{{#titleCase string}}
```

**Parameters:**
- `string` (string) - The string to convert

**Returns:**
Title case string

**Examples:**
```hbs
{{#titleCase "hello world"}}           {{!-- "Hello World" --}}
```

## `firstNonBlank`

Returns the string representation of the first argument that is not empty, whitespace, or null.

**Syntax:**
```hbs
{{#firstNonBlank values}}
{{#firstNonBlank value1 value2 value3 ...}}
```

**Parameters:**
- `value1, value2, value3 ...` (any) - Values to check in order

**Returns:**
First argument that is not empty, whitespace, or null, or null if all are blank.

**Examples:**
```hbs
{{#firstNonBlank model.name "Untitled"}}
```
