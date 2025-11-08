# Template Helper Functions

Kampose provides helper functions for Handlebars templates in themes. These functions simplify tasks like string manipulation, logical operations, and formatting, making template authoring easier.

Most helpers return values that can render directly or be used in Handlebars expressions. The helpers that render content directly to the output adjust to the current format and support all formats provided by Kampose.

## String Helpers

String manipulation and formatting functions that transform text values and return processed strings.

| Helper                                                           | Purpose                                                                | Parameters           | Returns    |
|------------------------------------------------------------------|------------------------------------------------------------------------|----------------------|------------|
| [`format`](template-helpers/string-helpers.md#format)               | Converts a value to a string with optional formatting                  | `value`, `format?`   | `string`   |
| [`upperCase`](template-helpers/string-helpers.md#uppercase)         | Converts a string to uppercase                                         | `text`               | `string`   |
| [`lowerCase`](template-helpers/string-helpers.md#lowercase)         | Converts a string to lowercase                                         | `text`               | `string`   |
| [`kebabCase`](template-helpers/string-helpers.md#kebabcase)         | Converts a string to kebab-case                                        | `text`               | `string`   |
| [`snakeCase`](template-helpers/string-helpers.md#snakecase)         | Converts a string to snake_case                                        | `text`               | `string`   |
| [`titleCase`](template-helpers/string-helpers.md#titlecase)         | Converts a string to title case                                        | `text`               | `string`   |
| [`split`](template-helpers/string-helpers.md#split)                 | Splits a string by a separator                                         | `input`, `separator` | `string[]` |
| [`concat`](template-helpers/string-helpers.md#concat)               | Concatenates string representation of some values into a single string | `...values`          | `string`   |
| [`firstNonBlank`](template-helpers/string-helpers.md#firstnonblank) | Returns the first non-blank value                                      | `...values`          | `string?`  |

## Logical Helpers

Conditional logic and comparison functions that evaluate expressions and return boolean values. These helpers support truthiness evaluation and allow controlling template flow with complex conditional logic.

| Helper                                        | Purpose                     | Parameters              | Returns   |
|-----------------------------------------------|-----------------------------|-------------------------|-----------|
| [`eq`](template-helpers/logical-helpers.md#eq)   | Tests equality              | `value1`, `value2`      | `boolean` |
| [`ne`](template-helpers/logical-helpers.md#ne)   | Tests inequality            | `value1`, `value2`      | `boolean` |
| [`lt`](template-helpers/logical-helpers.md#lt)   | Tests less than             | `value1`, `value2`      | `boolean` |
| [`le`](template-helpers/logical-helpers.md#le)   | Tests less than or equal    | `value1`, `value2`      | `boolean` |
| [`gt`](template-helpers/logical-helpers.md#gt)   | Tests greater than          | `value1`, `value2`      | `boolean` |
| [`ge`](template-helpers/logical-helpers.md#ge)   | Tests greater than or equal | `value1`, `value2`      | `boolean` |
| [`in`](template-helpers/logical-helpers.md#in)   | Tests value in collection   | `needle`, `...haystack` | `boolean` |
| [`not`](template-helpers/logical-helpers.md#not) | Logical NOT                 | `value`                 | `boolean` |
| [`and`](template-helpers/logical-helpers.md#and) | Logical AND                 | `...values`             | `boolean` |
| [`or`](template-helpers/logical-helpers.md#or)   | Logical OR                  | `...values`             | `boolean` |

## Arithmetic Helpers

Arithmetic helpers provide basic mathematical operations for integer values.

| Helper                                             | Purpose                                 | Parameters         | Returns   |
|----------------------------------------------------|-----------------------------------------|--------------------|-----------|
| [`inc`](template-helpers/arithmetic-helpers.md#inc)   | Increments a numeric value by one       | `value`            | `number`  |
| [`dec`](template-helpers/arithmetic-helpers.md#dec)   | Decrements a numeric value by one       | `value`            | `number`  |
| [`add`](template-helpers/arithmetic-helpers.md#add)   | Adds two numeric values                 | `value1`, `value2` | `number`  |
| [`sub`](template-helpers/arithmetic-helpers.md#sub)   | Subtracts a numeric value from another  | `value1`, `value2` | `number`  |
| [`mul`](template-helpers/arithmetic-helpers.md#mul)   | Multiplies two numeric values           | `value1`, `value2` | `number`  |
| [`div`](template-helpers/arithmetic-helpers.md#div)   | Divides one numeric value by another    | `value1`, `value2` | `number`  |
| [`mod`](template-helpers/arithmetic-helpers.md#mod)   | Modulus of two numeric values           | `value1`, `value2` | `number`  |
| [`abs`](template-helpers/arithmetic-helpers.md#abs)   | Absolute value of a numeric value       | `value`            | `number`  |

## URL Helpers

URL generation and manipulation functions that handle path resolution and fragment extraction.

| Helper                                                               | Purpose                                                       | Parameters | Returns   |
|----------------------------------------------------------------------|---------------------------------------------------------------|------------|-----------|
| [`rootUrl`](template-helpers/url-helpers.md#rooturl)                 | Returns the root URL of the site                              |            | `uri`     |
| [`rootRelativeUrl`](template-helpers/url-helpers.md#rootrelativeurl) | Converts site-relative URL to document-relative               | `uri`      | `uri`     |
| [`fragment`](template-helpers/url-helpers.md#fragment)               | Extracts fragment (anchor) from a URL for linking to sections | `uri`      | `string?` |

## Member Helpers

Member helpers work with .NET types, members, and references. These helpers provide language-specific rendering for code elements and provide proper URLs cross-referencing documentation pages.

| Helper                                                                    | Purpose                                                                            | Parameters | Returns   |
|---------------------------------------------------------------------------|------------------------------------------------------------------------------------|------------|-----------|
| [`memberDefinition`](template-helpers/member-helpers.md#memberdefinition) | Renders definition of a type or type member according to language syntax           | `element`  | N/A       |
| [`memberSignature`](template-helpers/member-helpers.md#membersignature)   | Renders signature of a namespace, type or type member according to language syntax | `element`  | N/A       |
| [`memberName`](template-helpers/member-helpers.md#membername)             | Returns name of a namespace, type or type member, including its declaring type     | `element`  | `string`  |
| [`memberUrl`](template-helpers/member-helpers.md#memberurl)               | Returns URL to documentation page of a namespace, type, or type member             | `element`  | `uri`     |
| [`memberCategory`](template-helpers/member-helpers.md#membercategory)     | Returns category of a namespace, type, or type member                              | `element`  | `string`  |

## Utility Helpers

General utility functions for common operations including value selection, numeric manipulation, date formatting, and content processing.

| Helper                                                           | Purpose                                                                            | Parameters                            | Returns   |
|------------------------------------------------------------------|------------------------------------------------------------------------------------|---------------------------------------|-----------|
| [`isUndefined`](template-helpers/utility-helpers.md#isUndefined) | Checks if a value is undefined                                                     | `value`                               | `boolean` |
| [`isNull`](template-helpers/utility-helpers.md#isNull)           | Checks if a value is null                                                          | `value`                               | `boolean` |
| [`isOdd`](template-helpers/utility-helpers.md#isOdd)             | Checks if an integer number is odd                                                 | `value`                               | `boolean` |
| [`select`](template-helpers/utility-helpers.md#select)           | Selects a value from choices based on a selector                                   | `selector`, `choice1`, `choice2`, ... | `any`     |
| [`len`](template-helpers/utility-helpers.md#len)                 | Returns the length of a string or array                                            | `value`                               | `number`  |
| [`now`](template-helpers/utility-helpers.md#now)                 | Returns current date/time with formatting                                          | `format?`                             | `string`  |
| [`json`](template-helpers/utility-helpers.md#json)               | Converts value to JSON string representation                                       | `value`                               | `string`  |
| [`literal`](template-helpers/utility-helpers.md#literal)         | Renders a value as a formatted literal constant in the target programming language | `value`                               | N/A       |
| [`cref`](template-helpers/utility-helpers.md#cref)               | Renders a link to the documentation of a code reference                            | `cref`                                | N/A       |
| [`markdown`](template-helpers/utility-helpers.md#markdown)       | Renders block content in Markdown as output format                                 | (block helper)                        | N/A       |
| [`stripTags`](template-helpers/utility-helpers.md#striptags)     | Removes HTML tags from block content                                               | (block helper)                        | N/A       |

