# Logical Helpers

Conditional logic and comparison functions that evaluate expressions and return boolean values. These helpers support truthiness evaluation and allow controlling template flow with complex conditional logic.

| Helper        | Purpose                     | Parameters              | Returns   |
|---------------|-----------------------------|-------------------------|-----------|
| [`eq`](#eq)   | Tests equality              | `value1`, `value2`      | `boolean` |
| [`ne`](#ne)   | Tests inequality            | `value1`, `value2`      | `boolean` |
| [`lt`](#lt)   | Tests less than             | `value1`, `value2`      | `boolean` |
| [`le`](#le)   | Tests less than or equal    | `value1`, `value2`      | `boolean` |
| [`gt`](#gt)   | Tests greater than          | `value1`, `value2`      | `boolean` |
| [`ge`](#ge)   | Tests greater than or equal | `value1`, `value2`      | `boolean` |
| [`in`](#in)   | Tests value in collection   | `needle`, `...haystack` | `boolean` |
| [`not`](#not) | Logical NOT                 | `value`                 | `boolean` |
| [`and`](#and) | Logical AND                 | `...values`             | `boolean` |
| [`or`](#or)   | Logical OR                  | `...values`             | `boolean` |

## Falsy and Truthy Values

In logical expressions, the following values are considered **falsy** and will evaluate to `false`:

- Null values
- False boolean values
- Zero length strings
- Zero values, either integer or float
- Empty arrays, collections, and enumerable objects
- Not a number (`NaN`)
- Undefined variables

Any other value is considered **truthy** and will evaluate to `true`.

## `eq`

Tests if two values are equal.

**Syntax:**
```hbs
{{#eq value1 value2}}
```

**Parameters:**
- `value1` (any) - First value to compare
- `value2` (any) - Second value to compare

**Returns:**
`true` if values are equal, `false` otherwise

**Examples:**
```hbs
{{#if (eq model.name "MyClass")}}
  <div class="my-class-documentation">
    {{model.doc.summary}}
  </div>
{{/if}}

{{#if (eq @index 0)}}
  <div class="first-item">
{{/if}}
```

## `ne`

Tests if two values are not equal.

**Syntax:**
```hbs
{{#ne value1 value2}}
```

**Parameters:**
- `value1` (any) - First value to compare
- `value2` (any) - Second value to compare

**Returns:**
`true` if values are not equal, `false` otherwise

**Examples:**
```hbs
{{#if (ne model.name "MyClass")}}
  <div class="other-class-documentation">
    {{model.doc.summary}}
  </div>
{{/if}}

{{#if (ne @index 0)}}
  <span class="separator">, </span>
{{/if}}
```

## `lt`

Tests if the first value is less than the second.

**Syntax:**
```hbs
{{#lt value1 value2}}
```

**Parameters:**
- `value1` (comparable) - First value to compare
- `value2` (comparable) - Second value to compare

**Returns:**
`true` if value1 < value2, `false` otherwise

**Examples:**
```hbs
{{#if (lt @index 5)}}
  <span class="limited-display">{{this}}</span>
{{/if}}

{{#if (lt model.parameters.count 3)}}
  <span class="simple-method">Simple</span>
{{/if}}
```

## `le`

Tests if the first value is less than or equal to the second.

**Syntax:**
```hbs
{{#le value1 value2}}
```

**Parameters:**
- `value1` (comparable) - First value to compare
- `value2` (comparable) - Second value to compare

**Returns:**
`true` if value1 <= value2, `false` otherwise

**Examples:**
```hbs
{{#if (le model.declaredMembers.count 10)}}
  <div class="show-all">
{{else}}
  <div class="show-summary">
{{/if}}
```

## `gt`

Tests if the first value is greater than the second.

**Syntax:**
```hbs
{{#gt value1 value2}}
```

**Parameters:**
- `value1` (comparable) - First value to compare
- `value2` (comparable) - Second value to compare

**Returns:**
`true` if value1 > value2, `false` otherwise

**Examples:**
```hbs
{{#if (gt model.declaredMethods.length 0)}}
  <section class="methods">
    <h2>Methods</h2>
{{/if}}

{{#if (gt @index 0)}}
  <hr class="separator">
{{/if}}
```

## `ge`

Tests if the first value is greater than or equal to the second.

**Syntax:**
```hbs
{{#ge value1 value2}}
```

**Parameters:**
- `value1` (comparable) - First value to compare
- `value2` (comparable) - Second value to compare

**Returns:**
`true` if value1 >= value2, `false` otherwise

**Examples:**
```hbs
{{#if (ge model.metadata.nestedTypes 1)}}
  <div class="nested-types">
{{/if}}

{{#if (ge @index 0)}}
  <div class="valid-index">
{{/if}}
```

## `in`

Tests if a value exists in a collection.

**Syntax:**
```hbs
{{#in value haystack}}
{{#in value value1 value2 value3 ...}}
```

**Parameters:**
- `value` (any) - Value to search for
- `haystack` (array) - Collection to search in
- `value1, value2, value3 ...` (any) - Values to check against

**Returns:**
`true` if value is found in collection, `false` otherwise

**Examples:**
```hbs
{{#if (in (format model.modelType) 'Class' 'Struct' )}}
  <div class="compound-type">
{{/if}}

{{#if (in (format model.modelType) allowedCategories)}}
  <div class="allowed-category">
{{/if}}
```

## `not`

Logical NOT operation.

**Syntax:**
```hbs
{{#not value}}
```

**Parameters:**
- `value` (any) - Value to negate

**Returns:**
`true` if value is falsy, `false` if value is truthy

**Examples:**
```hbs
{{#if (not model.metadata.isStatic)}}
  <div class="instance-member">
{{/if}}

{{#if (not @first)}}
  <span class="separator"> | </span>
{{/if}}
```

## `and`

Logical AND operation.

**Syntax:**
```hbs
{{#and values}}
{{#and value1 value2 value3 ...}}
```

**Parameters:**
- `values` (array) - Values to evaluate
- `value1, value2, value3 ...` (any) - Values to evaluate

**Returns:**
`true` if all values are truthy, `false` otherwise

**Examples:**
```hbs
{{#if (and model.metadata.isPublic model.isIndexer)}}
  <div class="documented-public-index-property">
{{/if}}

{{#if (and (gt @index 0) (lt @index 10))}}
  <div class="middle-range">
{{/if}}
```

## `or`

Logical OR operation.

**Syntax:**
```hbs
{{#or values}}
{{#or value1 value2 value3 ...}}
```

**Parameters:**
- `values` (array) - Values to evaluate
- `value1, value2, value3 ...` (any) - Values to evaluate

**Returns:**
`true` if any value is truthy, `false` otherwise

**Examples:**
```hbs
{{#if (or model.isStatic model.isReadOnly)}}
  <div class="special-member">
{{/if}}

{{#if (or @first @last)}}
  <div class="boundary-item">
{{/if}}
```
