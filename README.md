# Stop words
Redis has default list of stop words and they apply to all full-text indexes. These words are words that usually so common that they do not add much information to search, but take up a lot of space. When indexing, they are discarded. When searching, they are ignored: a, is, the, an, and, are, as, at, be, but, by, for, if, in, into, it, no, not, of, on, or, such, that, their, then, there, these, they, this, to, wa, will, with.

Add new stop words to index:
```
FT.CREATE myIndex STOPWORDS 3 foo bar test SCHEMA title TEXT body TEXT
```
Disable stop words:
```
Call FT.CREATE with STOPWORDS 0
```
# Dialects (default 1)
Dialects provide for enhancing the query API incrementally, introducing innovative behaviors and new features that support new use cases in a way that does not break the API for existing applications.

# Keyspace
Scan keyspace (scan for first 100 keys. returns cursor position):
SCAN 0 COUNT 100

Find keys whose name begins with "c":
SCAN 0 MATCH "c*" COUNT 100

Check if there is such key
EXISTS bicycle:0

Delete key
DEL bicycle:8

Set key with time to live (will be deleted after the time expires)
in seconds
SET test3 "LOCKED" EX 10
in milliseconds
SET test4 "LOCKED" PX 10000

Set expiration time (seconds)
EXPIRE test 20

How much time is left before a key expires 
(-1 - does not have expiration time, -2 - already expired)
TTL bicycle:1

# Data structures

## String

SET car "honda"
GET car

## Hash (equivalent of dictionaries or objects)

Create hash:
HSET bike:1 model Deimos brand Ergonom type "Enduro bikes" price 4972

Show all fields in hash:
HGETALL bike:1

Show particular field:
HGET bike:1 price

Update field:
HSET bike:1 model "Kraken" price 3000

Delete field (once all fields are removed, the hash key itself will be deleted):
HDEL bike:1 model

Increment and decrement:
HINCRBY bike:1 price 100
HINCRBY bike:1 price -100

## List

Create:
RPUSH bike:colors "Red" "Blue" "White" "Yellow"

Add to the end:
RPUSH bike:colors "White"

Add to the beginning:
LPUSH bike:colors "Red"

Delete:
DEL bike:colors

Remove item from the beginning:
LPOP bike:colors

Remove item from the end:
RPOP bike:colors

Number of elements:
LLEN bike:colors

Get elements
 - the index of the first element; note Redis lists use zero-based indexes
 - the index of the last element
For (2), a value of -1 means the last element in the list; -2 means the penultimate (second to last) element, and so on:
LRANGE bike:colors 1 2
LRANGE bike:colors 0 -1 (get all elements)
LRANGE bike:colors 1 -2

## Set (all elements must be unique)

Create:
SADD bike:1:addons "bell" "reflectors"

Add:
SADD bike:1:addons "tires"

Remove:
SREM bike:1:addons "bell"

## Sorted Set (all elements must be unique and have associated score, which provides the mechanism for sorting)

Create:
ZADD bike:brands 1940 "Alan Kay"
ZADD bike:brands 1906 "Grace Hopper"
ZADD bike:brands 1953 "Richard Stallman"
ZADD bike:brands 1965 "Yukihiro Matsumoto"
ZADD bike:brands 1916 "Claude Shannon"
ZADD bike:brands 1969 "Linus Torvalds"
ZADD bike:brands 1957 "Sophie Wilson"
ZADD bike:brands 1912 "Alan Turing"

Get all elements (they will be ordered by default): 
ZRANGE bike:brands 0 -1
ZRANGEBYSCORE bike:brands -inf +inf
or with score, showed in result
ZRANGEBYSCORE bike:brands -inf +inf WITHSCORES

Delete:
ZREM bike:brands "Alan Turing"

Get rank (index) and score of element:
ZRANK bike:brands "Yukihiro Matsumoto" WITHSCORE


## Numeric field

Exact match 
FT.SEARCH idx:bicycle "@price:[270 270]"
FT.SEARCH idx:bicycle "*" FILTER price 270 270

Start and end values are by default inclusive, but you can prepend ( to a value to exclude it from the range.

Finds where price >= 1200
FT.SEARCH idx:bicycle "@price:[1200 +inf]" RETURN 1 price

Finds where price > 1200
FT.SEARCH idx:bicycle "@price:[(1200 +inf]" RETURN 1 price

Finds where price <= 3200
FT.SEARCH idx:bicycle "@price:[-inf 3200]" RETURN 1 price

Finds where price < 3200
FT.SEARCH idx:bicycle "@price:[-inf (3200]" RETURN 1 price

## Tag field
A tag is a short sequence of text

Exact match
FT.SEARCH idx:bicycle "@condition:{new}"

Find part of text
FT.SEARCH idx:bicycle "@condition:{ne*}"

## Full-text field

Exact match
FT.SEARCH idx:bicycle "@model:\"Jigger\""

Find where field contains word (exactly word, not part of text). 
Redis doesn't index and find the most popular words like articles THE and A:
FT.SEARCH idx:bicycle "@model:Flow"

Find word across all full-text fields:
FT.SEARCH idx:bicycle "Velorim"

Find part of word:
FT.SEARCH idx:bicycle "*igg*"

Fuzzy search. 
A single pair represents a Levenstein distance of one. 
Two pairs represent a distance of two.
Three ones (maximum) represents a distance of three.
The Levenshtein distance is a string metric for measuring the difference between two sequences defined as the minimum number of edit operations:
FT.SEARCH idx:bicycle "%Jiger%"
FT.SEARCH idx:bicycle "%%Jier%%"
FT.SEARCH idx:bicycle "%%%Jer%%%"

# Use Redis as document database

Create index
FT.CREATE idx:bicycle ON JSON PREFIX 1 bicycle: SCORE 1.0 SCHEMA $.brand AS brand TEXT WEIGHT 1.0 $.model AS model TEXT WEIGHT 1.0 $.description AS description TEXT WEIGHT 1.0 $.price AS price NUMERIC $.condition AS condition TAG SEPARATOR .

Delete index
FT.DROPINDEX idx:bicycle

Add JSON document

JSON.SET "bicycle:0" "." "{\"brand\": \"Velorim\", \"model\": \"Jigger\", \"price\": 270, \"description\": \"Dunno\", \"condition\": \"new\"}"

Find documents

FT.SEARCH "idx:bicycle" "@brand:\"Velorim\"" LIMIT 0 10

# JSON

Create object:
JSON.SET bicycle:1 $ '{
    "model": "Jigger",
    "brand": "Velorim",
    "price": 270,
    "type": "Kids bikes",
    "specs": {
        "material": "aluminium",
        "weight": "10"
      },
    "description": "The Jigger is the best ride for the smallest of tikes!",
    "addons": [
        "reflectors",
        "grip tassles"
    ],
    "helmet_included": false
    }'

Get:
JSON.GET bicycle:1

Get parts using JSONPath:
JSON.GET bicycle:1 $.price
JSON.GET bicycle:1 $.specs.weight
JSON.GET bicycle:1 $.addons[0]

Get several objects:
JSON.MGET bicycle:1 bicycle:2 $

Get part of several objects:
JSON.MGET bicycle:1 bicycle:2 bicycle:3 $.price

Add new key-value in object:
JSON.SET bicycle:1 $.newmember '"value"'

Delete part from object:
JSON.DEL bicycle:1 $.newmember

Decrease number:
JSON.NUMINCRBY bicycle:1 $.price -10

Change value of boolean field (from true to false and the other way around):
JSON.TOGGLE bicycle:1 $.helmet_included

Replace array:
JSON.MERGE bicycle:1 $.addons '["reflectors", "rainbow seat"]'

Empty all arrays and set all numeric values to zero:
JSON.CLEAR doc $.*

## JSONPath

Get all members
JSON.GET doc $

Get member
JSON.GET doc $.a

# Time Series

Create
TS.CREATE bike_sales_1 DUPLICATE_POLICY SUM LABELS region east compacted no
TS.CREATE bike_sales_2 DUPLICATE_POLICY SUM LABELS region east compacted no

DUPLICATE_POLICY - defines the policy for handling insertion of multiple samples with identical timestamps.
LABELS - a set of label-value pairs that represent metadata labels for the key and serve as a secondary index.

Get information and statistics about about time series:
TS.INFO bike_sales_1

Get all time series matching region = west:
TS.QUERYINDEX region=east

Change time series:
TS.ALTER bike_sales_1 LABELS region east compacted no
TS.ALTER bike_sales_1 DUPLICATE_POLICY MIN

Add (timestamp - unix time in milliseconds, value)
TS.ADD bike_sales_1 1000 100

Get last sample in time series
TS.GET bike_sales_1 

Get last sample from all time series where region=east
TS.MGET FILTER region=east

Update (timestamp, new value)
TS.ADD bike_sales_1 1000 26 ON_DUPLICATE LAST

Delete all samples between two timestamps
TS.DEL bike_sales_1 999 1000

Delete single sample
TS.DEL bike_sales_1 1000 1000

Delete time series
DEL bike_sales_4

You can set retention period (when RETENTION set to 0, samples never expire. Default is zero)
TS.CREATE ts RETENTION 86400000 // samples expire after one full day

Get all samples from time series (you can see chart or points in Redis Insight)
TS.RANGE bike_sales_1 - +

Get average daily sales for a bike shop
TS.RANGE bike_sales_1 - + AGGREGATION avg 86400000

## Compactions

Create
TS.CREATE bike_sales_1_per_day LABELS region east compacted yes
TS.CREATERULE bike_sales_1 bike_sales_1_per_day AGGREGATION sum 86400000

## Arrays
Create object with array:
JSON.SET doc $ '{"a": [10, 20, 30, 40, 50]}'

Get length:
JSON.ARRLEN doc $.a

Add two values:
JSON.ARRAPPEND doc $.a 60 '"foo"'

Delete last item:
JSON.ARRPOP doc $.a

Trim array to the first 3 elements:
JSON.ARRTRIM doc $.a 0 2



# Analytic queries

Projection
FT.SEARCH "idx:bicycle" "*" RETURN 3 __key, brand, price

Apply mapping function
FT.AGGREGATE idx:bicycle "@condition:{new}" LOAD 2 "__key" "price" APPLY "@price - (@price * 0.1)" AS "discounted"
FT.AGGREGATE idx:bicycle "*" LOAD 2 "__key" "price" APPLY "@price - (@price * 0.1)" AS "discounted"

Grouping with aggregation
FT.AGGREGATE idx:bicycle "*" GROUPBY 1 @condition REDUCE SUM 1 "@price" AS "Sum"
FT.AGGREGATE idx:bicycle "*" GROUPBY 1 @condition REDUCE AVG 1 "@price" AS "average_price"
FT.AGGREGATE idx:bicycle "*" LOAD 1 price APPLY "@price<1000" AS price_category GROUPBY 1 @condition REDUCE SUM 1 "@price_category" AS "num_affordable"

Aggregation without grouping (for all documents)
FT.AGGREGATE idx:bicycle "*" APPLY "'bicycle'" AS type GROUPBY 1 @type REDUCE COUNT 0 AS num_total

Grouping without aggregation (comma-separated list of bicycles for each condition)
FT.AGGREGATE idx:bicycle "*" LOAD 1 "__key" GROUPBY 1 "@condition" REDUCE TOLIST 1 "__key" AS bicylces

|----------|---------------------------------------------------|
| used     | bicycle:1,bicycle:2,bicycle:3,bicycle:4           |
|----------|---------------------------------------------------|
| new      | bicycle:0,bicycle:5,bicycle:6,bicycle:8,bicycle:7 |
|----------|---------------------------------------------------|

# Operators

AND
FT.SEARCH idx:bicycle "@model:\"Jigger\" @price:[250 250]"

OR
FT.SEARCH idx:bicycle "@price:[250 250] | @price:[280 280]"

IN (for text)
FT.SEARCH idx:bicycle "@model:(Jigger | Test)"
FT.SEARCH idx:bicycle "(Jigger | Test)"

IN (for tags)
FT.SEARCH idx:bicycle "@condition:{new | old}"

NOT
FT.SEARCH idx:bicycle "-@condition:{new}"

# Sorting

FT.SEARCH idx:bicycle "*" RETURN 1 price SORTBY price ASC
FT.SEARCH idx:bicycle "*" RETURN 1 price SORTBY price DESC

# Limitation

Select 5 objects from 1
FT.SEARCH idx:bicycle "*" RETURN 1 price SORTBY price ASC LIMIT 1 5
