# Raw notes from brainstorming
Please refer to other docs primarily.

## Items

Items represent one physical thing out in the real world.
That may be a laptop or camera, a bag, a shelf in a cupboard, or the cupboard itself.

Items have a parent-child relationship in that items can contain other items.
Using the earlier examples, the camera may contain an SD card, the bag may contain the camera, and that bag could on on a shelf in the cupboard.

The inventory system would be able to track all of that, and tell you exactly where to go to find the SD card.

Some special items would represent buildings, geographic regions or countries, and the world itself, so that almost every item should have a parent set.


## Labels

Labels are attached to those items to be able to easily identify them, both for a person and for the inventory system.

Labels will contain some kind of machine-readable code, which may be a QR or other barcode, but it may also be an RFID tag, GS1 identifier (for medication), or something else.
Ideally, the inventory system should be agnostic to the exact storage method.

Optionally, the label will also contain a user-readable code, for use in case of an issue with the machine-readable code.

Ideally, QR codes generated should have usable URLs so they can be scanned without the inventory system's app installed, or without knowledge of the inventory system at all (for example, if a lost item is found).


## Scans

Scans represent each time the data on a label is read, and record details of that event.
The method of scanning, device performing the scan, and other appropriate metadata (such as geolocation from the device) should be captured.
This serves to provide a record of where the item has been or was found, and can help in the event that the location of the item was not correctly updated previously.


## Photos

Photos can be attached to items to provide extra data around visual appearance or location.


## Tags

Tags serve as ways to attach additional metadata to items.

One concept for their use was to have a plugin that used tags to store washing information for clothes, and a reader next to the washing machine that could scan an entire laundry load of RFID tags.
This could then identify the correct wash program, or highlight any problematic items that may need special treatment.

Another use could be to record IDs that reference other systems (such as log aggregation or monitoring for laptops).

Another use could be to record the expiry dates for food and produce a report of what foods are expiring soon, or to notify when inventorying that a food should be thrown away.


## Foreign Servers

Foreign servers represent other inventory systems.
The concept was to write an adapter layer so that if items from another system were scanned, some information on them could be fetched from the other system, and possibly a record of the scan pushed to the other system.


## AAA

Auth is to be handled entirely by Authentic where feasible, i.e. no passwords or 2FA tokens in the application itself.

The system is intended to be used primarily by me, but others may need access.

Other apps may need to request access tokens from Authentik for specific purposes that can be used to access the inventory system's APIs.


## Usage Patterns

The inventory system will be accessed from laptops for larger administration tasks, as well as from mobile devices for interacting with some labels and working "on the go".

For example, when looking at boxes in a warehouse, mobile signal may be difficult.


## Misc Features

Functionality is needed to "inventory" a container, wherein all the items found in the container are scanned.

This is cross-referenced to the expected contents, and a report generated as the user progresses.

Options would be given to move items unexpectedly found, or to record that an item is missing and that an alert should be displayed to update it when next scanned.

Such inventorying should still generate scan reports.


### External APIs

The API format for the "L" inventory system is unknown.
I've asked for information on this, but for now I think we make an interface that can be implemented to add adapters to other systems as needed.


### Fixed Scanners

I am planning on building fixed scanners for those, but not as part of the initial version.

### Barcode Formats

I also remembered another feature, some products (such as food) come with QR codes already on them, which have information about the product encoded.
Similar to the GS1 barcodes for medications, we should have an interface that can be implemented to decode this.
