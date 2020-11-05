# Sensing

Games drive on strong opponents, and rogue-like games even more so. 
Heroes roaming through dungeons rely on their vision and other senses
to detect monsters and secrets. And monsters? They can hear you coming.

As part of the global meta-system, all items define how they affect sensory 
data. Senses are simulated as a two part system. A `sense source` creates
signals that propagate through the map. `Sense receptors` (your eyes and ears) 
can detect those signals if in range. Walls and other dungeon inventory stands
between the signal source and a potential receptor. Most items in a dungeon
will either allow sensory data to pass freely or block the data to some varying 
degree.

## Sense Types

Out of the box, Rogue Entity supports four sense source types:

* Light
* Heat
* Noise
* Smell

Those senses can be classed into two main types: Radiation sense sources 
(light and heat) spread their signal in a straight line-of-sight fashion through
the surroundings. Lingering sense sources (smells, noise) spread as gases.

Stateful Entities (monsters, but also automatons etc) can have sense receptors.

As with sense sources, there are two classes of sense receptor behaviour based
on its sense signal physics. Receptors can either detect senses as radiation effects
or as lingering sense effects.

A radiation sense receptor passively observes its surrounding area. It establishes 
a line-of-sight field for each observed position and will detect any sense source
effect on any of the observed cells. This somewhat accurately simulates how 
vision works. The observer just needs to see the reflection of light on any visible
surface to perceive the location that is affected.

In contrast, a lingering sense receptor will only trigger if the sense signal reaches
the cell the sense receptor currently occupies. This simulates smells, heat and
noise sensors found in the real world. Once a signal has been received, the
receptor can tell the direction from which the sense has been triggered. 

Sense sources and sense receptor types can be freely combined: 

* **Normal** vision is simulated by combining a vision sense receptor (a 
  radiation-type sensor) with a light source (a radiation type source). 
* **InfraVision** combines heat sources with a heat-based radiation type receptor.
* **Heat-Sense** uses the same heat sources as InfraVision but uses a lingering-sense 
  type sense receptor. This effectively simulates a human's ability to sense heat 
  with their skin. The lingering nature of the sensor ensures heat is only detected
  when the receptor is in the area of effect of the heat source.
* **Smells** and **Noise** are detected as lingering sense sources using lingering sense
  receptors. This type of receptor correctly emulates the function of noses and ears.
  Smells and noises are modelled as lingering senses. Although sound behaves like
  a radiating source in the real world, sound waves are commonly reflected to make
  their effects appear to follow air currents more than line of sights. 
* **Touch** senses are somewhat special and act as fall-back sense for all entities that
  require some form of sensing to operate. Touch behaves as a radiating sense with
  an effective range of one cell and should be declared when any other sense is used.
  Touch receptor entities are also touch sense sources. Touch sense cannot be 
  blocked.

## Declaring Sense Sources

All sense source entities must be modelled as stateful entities. Bulk-Entities exist
outside of the entity system and thus cannot be efficiently included in the calculations.

A entity that can potentially act as sense source must declare a corresponding 
sense source trait. Sense sources do not publish state data. A sense source defines an 
sense source intensity, which indirectly affects the sense's area of effect. 

A sense source's effective radius, signal decay and method of signal spreading is defined
by a corresponding sense physics configuration. Sense physics configurations are shared
across all sense sources of the same type.

## Declaring Sense Receptors

To detect sense signals an entity must declare an `sense receptor` trait. A sense receptor 
establishes an effective area of effect similar to sense sources that is used to detect incoming
signals.

During sense detection, the receptor's field of view is combined with any data source within
the receptor's sense area. Each sense type defines its own method of providing sense mapping 
information. Each of these maps is a view of effective sense strength and sense direction as
perceived by the sensing entity.

## Global Sense Maps

The system can optionally compute global sense maps. These maps are compiled from all 
active sense sources without any filtering from sense receptors. These sense maps should
not be used for entity systems but can be helpful to create observer or debug views.

# Sense Data Propagation

Entities that are neither sense sources nor sense receptors can still affect sense calculations.

Walls are commonly known to block light, heat, smells and even noise. An entity's sense blocking
ability is defined by declaring a sense resistance trait on the entity. This trait can be defined 
on both stateful reference entities and stateless bulk-entities. 

A ``SensePropertiesMap`` holds information on how sensory information spreads through the 
world and how entities influence this spread. For light, this map collects translucency, for heat 
it collects an insulation factor etc. 

The sense resistance mapping system precomputes a ``SensePropertiesMap`` from all entities 
available on the map and combines all information of all relevant map layers into a global view.

Change notifications from to the map system trigger a re-computation of this information before
sense sources or receptors are updated.

It is generally a good idea to define sense resistances for the environment entities of a world.
The system is flexible enough to handle changes to the environment. Actor entities (monsters, 
players) are moving frequently and can put extra cost on the map update system due to the
frequent position changes. 


# Sense Processing

Processing of sensing information for actors is a two stage process.

Sense sources create signals in the world. Those signals are global effects
and are available to all actors that can potentially observe these signals. 
This means they are calculated once per frame, and can be heavily cached 
if the source is constant (does not change position or intensity).

Most sense types can be defined simply by the interaction between a sense
source and a sense receptor. A noise forms a relationship between the
noise source and the actor hearing the noise. 




---

Sense Caching:

- Tracks cache validation information for sense sources and sense receptors.
- Two tracks:
  - Global: Cache information from map updates that influence how sense data 
            travels through map cells. This in return affects sense sources.
            Any active sense source affected by such a change will in turn 
            mark the sense receptor cache as invalid.
  - ReceptorCache:
            Tracks the validity of sense sources. This information is updated
            when sense sources are computed. It tracks changes to sense source
            emissions and sense source positions.
