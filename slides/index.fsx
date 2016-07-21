(**
- title : Domain Modeling with Types
- description : Domain Modeling with Types (using F#)
- author : Ryan Riley (@panesofglass)
- theme : night
- transition : default

***

# Domain Modeling with Types
<a href="http://fsharp.org/"><img alt="F# Software Foundation" src="images/fssf.png" /></a>

***

## Ryan Riley
<img alt="Tachyus logo" src="images/tachyus.png" style="background-color:#fff;" />
### [@panesofglass](https://twitter.com/panesofglass)
### [github/panesofglass](https://github.com/panesofglass)

***

# Objective

## Visualize distance between two cities by their geographic coordinates.

***

# Process

*)

(*** include: distance-calculator-example ***)

(**

' This pipeline shows that we want to take a list of two Cities as inputs,
' translate those cities into locations,
' and finally calculate the distance (in feet).

***

# Goal 1

## Define `City`

***

## Simplest thing possible (C#)

    [lang=cs]
    using City = string;

***

## Simplest thing possible (F#)

*)

module Alias =
    type City = string

(**
***

## Does this really help?

' Maybe: you get better documentation throughout your code;
' however, you don't get any type safety.

***

## Can we do better?

***

## Single-case union types

*)

type SingleCaseCity = CityName of string

(** Extract the value via pattern matching: *)
let city = CityName "Houston, TX"
let cityName (CityName name) = name
(*** define-output: result ***)
cityName city

(** Value of result: *)
(*** include-it: result ***)

(**
***

# Goal 2

## Define a `Location` type

***

## Option 1: Is-A

' A very common solution is to use the is-a approach,
' in which you declare something is some other thing.

***

## `ILocatable` interface

    [lang=cs]
    public interface ILocatable {
        float Latitude { get; }
        float Longitude { get; }
    }

' Is-a usually involves defining interfaces.
' The interface, common in C#, Java, and now TypeScript,
' doesn't define anything other than a contract to be implemented.

***

## `Place` type (C#)

    [lang=cs]
    public class Place : ILocatable {
        public City Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

' Here's a Place type in C# using the ILocatable interface
' and auto-generated properties. Note that many OO languages
' allow you to implicitly implement interfaces, as I've done
' here.

***

## `ILocatable` interface (F#)

*)

type ILocatable =
    abstract Latitude : float
    abstract Longitude : float

(**
***

## `Place` type (F#)

*)

module FirstTry =

    type Place =
        { Name : string
          Latitude : float
          Longitude : float }
        interface ILocatable with
            member this.Latitude = this.Latitude
            member this.Longitude = this.Longitude

(**

' Here's a similar implementation in F#.
' F# forces explicit interface implementation. Further,
' F# does not implicitly cast to an interface, so you have
' to do a bit more work. In other words, F# forces you to
' be explicit with your types. As we'll see, this can be
' a good way to encourage you to move to better practices.

***

## What could go wrong?

1. Latitude or longitude not provided
2. Latitude and longitude mixed up
3. Invalid city and location values

***

## 1. Handling missing values

Examples:

* Atlantis
* Camelot

***

## C# version:

    [lang=cs]
    class Locatable : ILocatable {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class Place {
        public City Name { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public ILocatable GetLocation() {
            if (Latitude.HasValue && Longitude.HasValue) {
                return new Locatable {
                    Latitude = Latitude.Value,
                    Longitude = Longitude.Value
                };
            } else {
                return null; // Oh noes!
            }
        }
    }

***

## F# `Place` with optional `ILocatable`

*)

module OptionalLocation =

    type Place =
        { Name : string
          Latitude : float option
          Longitude : float option }
        member this.GetLocation() : ILocatable option =
            match this.Latitude, this.Longitude with
            | Some lat, Some lng ->
                { new ILocatable with
                    member this.Latitude = lat
                    member this.Longitude = lng } |> Some
            | _ -> None

(**
***

## How would you handle?

1. supporting both surface and downhole locations
2. adding x/y coordinate locations for grid rendering

***

## Is-A Falls Apart

    [lang=cs]
    public interface IDownhole {
        float DHLatitude { get; }
        float DHLongitude { get; }
    }
    public interface IXYCoord {
        float X { get; }
        float Y { get; }
    }
    public class Place : ILocatable, IDownhole, IXYCoord {
        public City Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float DHLatitude { get; set; }
        public float DHLongitude { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }

' This goes back to the less rigorous approach simply to try to fit it on the screen.
' Note the weird names for DHLatitude and DHLongitude. We probably wouldn't do that,
' but what might you do instead? Naming is already hard, and you typically find strange
' things like this in mature OO codebases. At least, I have. And I've written such
' craziness in the past for lack of a better idea.

---

## Can at least create a new instance with all values.

    [lang=cs]
    var place = new Place {
        Name = "Houston, TX",
        Latitude = 29.760427,
        Longitude = -95.369803,
        DHLatitude = 29.760445,
        DHLongitude = -95.369798,
        X = 100,
        Y = 150
    };

---

## Is-A Fails

    [lang=cs]
    public interface IDownhole {
        float Latitude { get; }
        float Longitude { get; }
    }
    public class Place : ILocatable, IDownhole, IXYCoord {
        public City Name { get; set; }
        public float ILocatable.Latitude { get; set; }
        public float ILocatable.Longitude { get; set; }
        public float IDownhole.Latitude { get; set; }
        public float IDownhole.Longitude { get; set; }
        public float IXYCoord.X { get; set; }
        public float IXYCoord.Y { get; set; }
    }

---

## Lose ability to create complete instance

    [lang=cs]
    // Can only set City when creating a new Place.
    var place = new Place { Name = "Houston, TX" };
    // Cast to set remaining properties.
    var locatable = (ILocatable)place;
    locatable.Latitude = 29.760427;
    locatable.Longitude = -95.369803<degLng>

' Now we've done it! We lost nearly all the benefits in order to use
' better names. This really is not working well.

***

## Can we do better?

***

## Rethinking `ILocatable`

### "Is-a" vs. "Has-a"

***

## `Latitude` and `Longitude` belong together

***

## Refactored `Place` type (C#)

    [lang=cs]
    public class Location {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class Place {
        public City Name { get; set; }
        public Location Location { get; set; }
    }

***

## Note: "optional" indicated by a `null`

***

## Re-refactored `Place` type (C#)

    [lang=cs]
    public struct Location {
        public float Latitude;
        public float Longitude;
    }

    public class Place {
        public City Name { get; set; }
        public Nullable<Location> Location { get; set; }
    }

' We can fix the null by using a struct. In this case, the use
' of a struct and Nullable will work out alright. However, there
' are tradeoffs to using a struct that must be considered for
' each use case.

***

## Refactored `Place` type (F#)

*)

module Revised =

    type Location = { Latitude : float; Longitude : float }

    type Place = { Name : string; Location : Location option }

(**
***

## Note: No `null`

***

## How would you handle?

1. supporting both surface and downhole locations
2. adding x/y coordinate locations for grid rendering

***

## Extended C# `Place`

    [lang=cs]
    public struct XYCoord {
        public float X;
        public float Y;
    }
    public class Place {
        public City Name { get; set; }
        public Nullable<Location> Surface { get; set; }
        public Nullable<Location> Downhole { get; set; }
        public Nullable<XYCoord> Coord { get; set; }
    }

***

## Extended F# `Place`

*)

module ExtendedExample =
    type Location = { Latitude : float; Longitude : float }
    type XYCoord =  { X: float; Y: float }
    type Place = {
        Name : string
        Surface : Location option
        Downhole : Location option
        Coord : XYCoord option
    }

(**
***

## Has-a preferred to Is-a

### "Composition over inheritance"

***

## 2. Mixing up latitude and longitude

***

## Sadly we will have to leave C# behind

***

## Units of measure

*)

(*** include: location-measure-types ***)

(**
***

## Correct `Location`

*)

(*** include: location2 ***)

(**
***

## [Make illegal states unrepresentable](http://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/)

***

## Types can only get you so far

1. `City` allows `null` and `""`
2. `Location` allows latitude < -90 and > 90
3. `Location` allows longitude < -180 and > 180

***

## 3. Valid states

***

## Revisiting `City`

*)

(*** include: city ***)

(**

' Without a city lookup, we are stuck with a minimal
' validation for the city name. We could add a bit
' more, such as requiring one or more commas, but
' this provides the general idea.

***

## Better than nothing

***

## Revisiting `Location`

*)

(*** include: location ***)

(**

' This may look strange as we are now hiding
' the record we previously used. However, we
' retain immutability and add validation.

***

## Revisiting `Place`

*)

(*** include: place ***)

(**
***

## `Place` still has yet to change

### Single Responsibility Principle

' Place doesn't really change except to swap
' Name : string for Name : City
' This is exactly the goal of SRP.

***

## Further study: Contracts and Dependent Types

* [Code Contracts](http://research.microsoft.com/en-us/projects/contracts/)
* [F*](https://fstar-lang.org/)
* [TS*](http://research.microsoft.com/en-us/um/people/nswamy/papers/tstar.pdf)
* [Idris](http://www.idris-lang.org/)

' We have seen that the F# type system can
' get us pretty far along our way. 
' We had to resort to constructor functions
' to provide more rigid validation.

' Note that we can still create an invalid `City`.
' However, if we privatize the union, we can't
' retrieve the name via pattern matching.

' Some languages offer additional type system
' constructs to do these validations. Code contracts,
' originally from Eiffel, offer one form of this.
' Newer languages have also been working on a concpt
' called dependent types or refinement types. These
' allow you to further specify the range of values
' allowed, as we have done above using our
' constructor functions. I've listed a few examples
' in case you wish to study these further.

***

# Goal 3: Processing Requests

***

## Process

*)

(*** include: distance-calculator-example ***)

(**

' This pipeline shows that we want to take a list of two Cities as inputs,
' translate those cities into locations,
' and finally calculate the distance (in feet).

***

## How do we translate?

* User input: two cities
* Calculation input: two places
* Unit conversion: meters -> feet

***

## Start with types

*)

(*** include: workflow-process-types ***)

(**
***

## Lookup coordinates by city name

*)

(*** include: lookup-locations ***)
(*** include: lookup-houston ***)
(*** include-it: lookup-houston-result ***)

(**

***

## Calculate Distance

*)

(*** include: find-distance ***)

(**

' System.Device.Location.GeoCoordinate has a method
' that uses the Haversine formula, which assumes a
' spherical earth. We could implement this ourselves,
' but why bother when it's already available?

***

## Unit conversion

*)

(*** include: units-of-measure ***)
(*** include: meters-to-feet ***)

(**

' Similar to what we saw above, we apply units of measure
' to ensure our results are what we intended. Note that
' here we create a conversion function to handle the conversion.

***

## Wrapping up the steps

*)

(*** include: distance-calculator-pipeline ***)
(*** include: run-distance-workflow ***)
(*** include-it: run-distance-workflow-result ***)

(**

' Here we use function composition and rely on type
' inference to create a single function to run our
' workflow.

***

# Review

***

## 1. Make illegal states unrepresentable

' The ultimate goal of leveraging a type system should
' be to make invalid states impossible. If you use types
' only for documentation, then you are likely just adding
' noise to your code. Leverage the compiler to cover as
' many of your test cases as possible and avoid the pain
' of debugging or scrambling to fix a production issue.

' Perhaps the best example of where developers failed
' was the Mars Climate Orbiter, which was lost because
' of a units error. https://d2cj35nmzi9erd.cloudfront.net/msp98/news/mco990930.html

***

## 2. Types define data flow

' Once you have the functions to support each step of your
' program, it's a simple matter of applying composition to
' create a single function to run your program. Use types
' to ensure the data flows through your program correctly.

***

## 3. Types can't always get you all the way

' Despite the extent to which we were able to add compile-time
' type checking throughout this little sample, we also saw
' a few cases where types were not able to help us, at least
' not in C# or F#. Some other languages and tools are paving
' the way for improvements, though, and perhaps we'll eventually
' see things like refinement and dependent types in .NET.

' Network interruptions, external dependencies, and more are
' still outside the scope of nearly all type systems. We have
' a long way to go, but we should at least start leveraging
' what we have available today.

***

# Resources

* [F# Software Foundation](http://fsharp.org/)
* [F# for Fun and Profit](http://fsharpforfunandprofit.com/)
* [Defunctionalization](http://www.brics.dk/RS/01/23/)
* [Search "F# domain driven design"](https://www.bing.com/search?q=F%23+domain+driven+design)

***

# Questions?

*)

(*** define: location-measure-types ***)
[<Measure>] type degLat
[<Measure>] type degLng

let degreesLatitude x = x * 1.<degLat>
let degreesLongitude x = x * 1.<degLng>

(*** hide ***)
let degLatResult = degreesLatitude 1.
let degLngResult = degreesLongitude 1.

module UnitsOfMeasure =
(*** define: location2 ***)
    type City = SingleCaseCity

    type Location = {
        Latitude : float<degLat>
        Longitude : float<degLng>
    }

    type Place = { Name : City; Location : Location option }

(*** define: city ***)
type City = City of name : string
    with
    static member Create (name : string) =
        match name with
        | null | "" ->
            invalidArg "Invalid city"
                "The city name cannot be null or empty."
        | x -> City x

(*** define: location ***)
type Location =
    internal { latitude : float<degLat>; longitude : float<degLng> }
    member this.Latitude = this.latitude
    member this.Longitude = this.longitude
    static member Create (lat, lng) =
        if -90.<degLat> > lat || lat > 90.<degLat> then
            invalidArg "lat"
                "Latitude must be within the range -90 to 90."
        elif -180.<degLng> > lng || lng > 180.<degLng> then
            invalidArg "lng"
                "Longitude must be within the range -180 to 180."
        else { latitude = lat; longitude = lng }
(*** hide ***)
    override this.ToString() =
        sprintf "lat %f, lng %f" this.Latitude this.Longitude

(*** define: place ***)
type Place = { Name : City; Location : Location option }

(*** define: units-of-measure ***)
[<Measure>] type m
[<Measure>] type ft

(*** define: workflow-process-types ***)
type LookupLocations = City * City -> Place * Place

type TryFindDistance = Place * Place -> float<m> option

type MetersToFeet = float<m> -> float<ft>

type Show = Place * Place * float<ft> option -> string

(*** define: haversine distance ***)
module GeoCoordinate =
    open System

    [<Measure>] type rad
    let private degreesToRadians (value:float<_>) = value * (Math.PI / 180.0)
    let private degLatToRad (lat:float<degLat>) = lat * (degreesToRadians 1.0<rad/degLat>)
    let private degLngToRad (lng:float<degLng>) = lng * (degreesToRadians 1.0<rad/degLng>)
    let private haversine (theta:float<rad>) = 0.5 * (1.0 - Math.Cos(theta/1.0<rad>))
    type private Pos = { phi : float<rad>; psi : float<rad> }
    let private toPos (a:Location) = { phi = degLatToRad a.Latitude; psi = degLngToRad a.Longitude }
    let private sub (a:Pos) (b:Pos) = { phi = a.phi - b.phi; psi = a.psi - b.psi }
    // https://en.wikipedia.org/wiki/Earth_radius#Mean_radius 
    let private earthRadius = 6371008.8<m>

    let distance a b =
        let a, b = toPos a, toPos b
        let h = haversine(b.phi - a.phi) +
                haversine(b.psi - a.psi) *
                Math.Cos(a.phi/1.<rad>) * Math.Cos(b.phi/1.<rad>)
        2. * earthRadius * Math.Asin(Math.Sqrt(h))

(*** define: locations ***)
let locations : Place list =
    [ { Name = City "Beaumont, TX"; Location = Some(Location.Create(30.080174<degLat>, -94.126556<degLng>)) }
      { Name = City "College Station, TX"; Location = Some(Location.Create(30.627977<degLat>, -96.334407<degLng>)) }
      { Name = City "Conroe, TX"; Location = Some(Location.Create(30.311877<degLat>, -95.456051<degLng>)) }
      { Name = City "Friendswood, TX"; Location = Some(Location.Create(29.529400<degLat>, -95.201045<degLng>)) }
      { Name = City "Houston, TX"; Location = Some(Location.Create(29.760427<degLat>, -95.369803<degLng>)) }
      { Name = City "Humble, TX"; Location = Some(Location.Create(29.998831<degLat>, -95.262155<degLng>)) }
      { Name = City "Huntsville, TX"; Location = Some(Location.Create(30.723526<degLat>, -95.550777<degLng>)) }
      { Name = City "Katy, TX"; Location = Some(Location.Create(29.785785<degLat>, -95.824396<degLng>)) }
      { Name = City "Pearland, TX"; Location = Some(Location.Create(29.563567<degLat>, -95.286047<degLng>)) }
      { Name = City "Spring, TX"; Location = Some(Location.Create(30.079940<degLat>, -95.417160<degLng>)) }
      { Name = City "The Woodlands, TX"; Location = Some(Location.Create(30.1435<degLat>, -95.4760<degLng>)) }
      { Name = City "San Mateo, CA"; Location = Some(Location.Create(37.5599<degLat>, -122.3131<degLng>)) }
      { Name = City "London, UK"; Location = Some(Location.Create(51.5179<degLat>, 0.1022<degLng>)) }
      { Name = City "Paris, FR"; Location = Some(Location.Create(48.856614<degLat>, 2.352222<degLng>)) }
      { Name = City "Ciudad Mitad del Mundo, Equador"; Location = Some(Location.Create(-0.0022<degLat>, -78.4558<degLng>)) }
      { Name = City "Durban, SA"; Location = Some(Location.Create(-29.8492<degLat>, 30.9873<degLng>)) }
      { Name = City "Adelaide, AUS"; Location = Some(Location.Create(-34.9261<degLat>, 138.5999<degLng>)) }
      { Name = City "Atlantis"; Location = None }
      { Name = City "Camelot"; Location = None }
    ]

(*** define: lookup-locations ***)
let lookupLocation city =
    locations |> List.find (fun x -> x.Name = city)

let lookupLocations (start, dest) =
    lookupLocation start, lookupLocation dest

(*** define: lookup-houston ***)
lookupLocation (City "Houston, TX")
(*** define-output: lookup-houston-result ***)
lookupLocation (City "Houston, TX")

(*** hide ***)
lookupLocation (City "Conroe, TX")
lookupLocation (City "The Woodlands, TX")
lookupLocation (City "San Mateo, CA")
lookupLocation (City "Adelaide, AUS")
lookupLocation (City "Atlantis")

(*** define: find-distance ***)
let findDistance (start: Location, dest: Location) =
    GeoCoordinate.distance start dest
    
let tryFindDistance : TryFindDistance = function
    | { Location = Some start }, { Location = Some dest } ->
        findDistance (start, dest) |> Some
    | _, _ -> None

(*** define: meters-to-feet ***)
let metersToFeet (input: float<m>) =
    input / 0.3048<m/ft>

(*** define: distance-calculator-pipeline ***)
let workflow =
    lookupLocations
    >> tryFindDistance
    >> Option.map metersToFeet

(*** define: distance-calculator-example ***)
(City "Houston, TX", City "San Mateo, CA")
|> lookupLocations
|> tryFindDistance
|> Option.map metersToFeet

(*** define: run-distance-workflow ***)
workflow (City "Houston, TX", City "San Mateo, CA")

(*** define-output: run-distance-workflow-result ***)
workflow (City "Houston, TX", City "San Mateo, CA")

(*** define: serialize ***)
let serializePlace = function
    | { Place.Name = City name; Location = Some loc } ->
        sprintf """{"name":"%s","location":{"latitude":%f,"longitude":%f}}""" name loc.Latitude loc.Longitude
    | _ -> "null"

let serializeResult place1 place2 distance =
    let place1' = serializePlace place1
    let place2' = serializePlace place2
    match distance with
    | Some d ->
        sprintf """{"start":%s,"dest":%s,"distance":%f}""" place1' place2' (d/1.<ft>)
    | None -> sprintf """{"start":%s,"dest":%s}""" place1' place2'

(*** define: distance-workflow ***)
let rec receiveInput(start, dest) =
    let start', dest' = lookupLocations(start, dest)
    calculateDistance(start', dest')
and calculateDistance(start, dest) =
    match tryFindDistance(start, dest) with
    | Some distance ->
        showPlacesWithDistanceInMeters(start, dest, distance)
    | None -> showPlaces(start, dest, None)
and showPlacesWithDistanceInMeters(start, dest, distance) =
    showPlaces(start, dest, Some(metersToFeet distance))
and showPlaces(start, dest, distance) =
    serializeResult start dest distance

(*** hide ***)
receiveInput(City "Houston, TX", City "San Mateo, CA")

(*** define: stages ***)
type Stage =
    | AwaitingInput
    | InputReceived of start : City * dest : City
    | Located of start : Place * dest : Place
    | Calculated of Place * Place * float<m> option
    | Show of Place * Place * float<ft> option

(*** define: processing-stages ***)
type RunWorkflow = Stage -> string

let rec runWorkflow = function
    | AwaitingInput -> runWorkflow AwaitingInput
    | InputReceived(start, dest) ->
        runWorkflow (Located(lookupLocations(start, dest)))
    | Located(start, dest) ->
        runWorkflow (Calculated(start, dest, tryFindDistance(start, dest)))
    | Calculated(start, dest, distance) ->
        runWorkflow (Show(start, dest, distance |> Option.map metersToFeet))
    | Show(start, dest, distance) -> serializeResult start dest distance

(*** hide ***)
runWorkflow (InputReceived(City "Houston, TX", City "San Mateo, CA"))
