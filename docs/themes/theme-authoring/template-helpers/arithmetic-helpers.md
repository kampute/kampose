# Arithmetic Helpers

Arithmetic helpers provide basic mathematical operations for integer values.

| Helper          | Purpose                                 | Parameters         | Returns   |
|-----------------|-----------------------------------------|--------------------|-----------|
| [`inc`](#inc)   | Increments a numeric value by one       | `value`            | `number`  |
| [`dec`](#dec)   | Decrements a numeric value by one       | `value`            | `number`  |
| [`add`](#add)   | Adds two numeric values                 | `value1`, `value2` | `number`  |
| [`sub`](#sub)   | Subtracts a numeric value from another  | `value1`, `value2` | `number`  |
| [`mul`](#mul)   | Multiplies two numeric values           | `value1`, `value2` | `number`  |
| [`div`](#div)   | Divides one numeric value by another    | `value1`, `value2` | `number`  |
| [`mod`](#mod)   | Modulus of two numeric values           | `value1`, `value2` | `number`  |
| [`abs`](#abs)   | Absolute value of a numeric value       | `value`            | `number`  |

## `inc`

Increments a numeric value by one.

**Syntax:**
```hbs
{{#inc value}}
```

**Parameters:**
- `value` (number) - The value to increment

**Returns:**
The incremented value as an integer.

**Examples:**
```hbs
{{#inc 5}}                     {{!-- 6 --}}
{{#inc @index}}                {{!-- Current index + 1 --}}
```

## `dec`

Decrements a numeric value by one.

**Syntax:**
```hbs
{{#dec value}}
```

**Parameters:**
- `value` (number) - The value to decrement

**Returns:**
The decremented value as an integer.

**Examples:**
```hbs
{{#dec 5}}                     {{!-- 4 --}}
{{#dec @index}}                {{!-- Current index - 1 --}}
```

## `add`

Adds two numeric values together.

**Syntax:**
```hbs
{{#add value1 value2}}
```

**Parameters:**
- `value1` (number) - The first value to add
- `value2` (number) - The second value to add

**Returns:**
The sum of the two values as an integer.

**Examples:**
```hbs
{{#add 5 10}}                  {{!-- 15 --}}
{{#add @index 2}}              {{!-- Current index + 2 --}}
```

## `sub`

Subtracts the second numeric value from the first.

**Syntax:**
```hbs
{{#sub value1 value2}}
```

**Parameters:**
- `value1` (number) - The first value
- `value2` (number) - The second value

**Returns:**
The difference between the two values as an integer.

**Examples:**
```hbs
{{#sub 10 5}}                   {{!-- 5 --}}
{{#sub @index 1}}               {{!-- Current index - 1 --}}
```

## `mul`

Multiplies two numeric values together.

**Syntax:**
```hbs
{{#mul value1 value2}}
```

**Parameters:**
- `value1` (number) - The first value to multiply
- `value2` (number) - The second value to multiply

**Returns:**
The product of the two values as an integer.

**Examples:**
```hbs
{{#mul 5 10}}                  {{!-- 50 --}}
{{#mul @index 2}}              {{!-- Current index * 2 --}}
```

## `div`

Divides the first numeric value by the second.

**Syntax:**
```hbs
{{#div value1 value2}}
```

**Parameters:**
- `value1` (number) - The numerator
- `value2` (number) - The denominator

**Returns:**
The result of the division as an integer.

**Examples:**
```hbs
{{#div 10 5}}                   {{!-- 2 --}}
{{#div @index 2}}               {{!-- Current index / 2 --}}
```

## `mod`

Calculates the modulus of the first numeric value by the second.

**Syntax:**
```hbs
{{#mod value1 value2}}
```

**Parameters:**
- `value1` (number) - The dividend
- `value2` (number) - The divisor

**Returns:**
The remainder of the division as an integer.

**Examples:**
```hbs
{{#mod 10 3}}                   {{!-- 1 --}}
{{#mod @index 2}}               {{!-- Current index % 2 --}}
```

## `abs`

Calculates the absolute value of a numeric value.

**Syntax:**
```hbs
{{#abs value}}
```

**Parameters:**
- `value` (number) - The value to calculate the absolute value of

**Returns:**
The absolute value of the input as an integer.

**Examples:**
```hbs
{{#abs -5}}                     {{!-- 5 --}}
{{#abs 10}}                     {{!-- 10 --}}
```
