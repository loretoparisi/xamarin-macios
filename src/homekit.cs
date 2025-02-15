using XamCore.CoreLocation;
using XamCore.ObjCRuntime;
using XamCore.Foundation;
using System;
using System.ComponentModel;

namespace XamCore.HomeKit {

	[iOS (8,0)]
	[Static]
	public partial interface HMErrors {
		[Field ("HMErrorDomain")]
		NSString HMErrorDomain { get; }
	}

	[iOS (8,0)]
	[BaseType (typeof (NSObject), Delegates=new string[] {"WeakDelegate"}, Events=new Type[] {typeof(HMHomeManagerDelegate)})]
	public partial interface HMHomeManager {

		[Export ("delegate", ArgumentSemantic.Weak)]
		[NullAllowed]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakDelegate")]
		[Protocolize]
		HMHomeManagerDelegate Delegate { get; set; }

		[Export ("primaryHome", ArgumentSemantic.Retain)]
		HMHome PrimaryHome { get; }

		[Export ("homes", ArgumentSemantic.Copy)]
		HMHome [] Homes { get; }

		[NoWatch]
		[Async]
		[Export ("updatePrimaryHome:completionHandler:")]
		void UpdatePrimaryHome (HMHome home, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("addHomeWithName:completionHandler:")]
		void AddHome (string homeName, Action<HMHome, NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeHome:completionHandler:")]
		void RemoveHome (HMHome home, Action<NSError> completion);
	}

	[iOS (8,0)]
	[Model, Protocol]
	[BaseType (typeof (NSObject))]
	public partial interface HMHomeManagerDelegate {

		[Export ("homeManagerDidUpdateHomes:")]
		void DidUpdateHomes (HMHomeManager manager);

		[Export ("homeManagerDidUpdatePrimaryHome:")]
		void DidUpdatePrimaryHome (HMHomeManager manager);

		[Export ("homeManager:didAddHome:"), EventArgs ("HMHomeManager")]
		void DidAddHome (HMHomeManager manager, HMHome home);

		[Export ("homeManager:didRemoveHome:"), EventArgs ("HMHomeManager")]
		void DidRemoveHome (HMHomeManager manager, HMHome home);
	}

	[iOS (8,0)]
	[BaseType (typeof (NSObject), Delegates=new string[] {"WeakDelegate"}, Events=new Type[] {typeof(HMAccessoryDelegate)})]
	public partial interface HMAccessory {

		[Export ("name")]
		string Name { get; }

		[NoWatch]
		[Availability (Introduced = Platform.iOS_8_0, Deprecated = Platform.iOS_9_0)]
		[Export ("identifier", ArgumentSemantic.Copy)]
		NSUuid Identifier { get; }

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }

		[Export ("delegate", ArgumentSemantic.Weak)]
		[NullAllowed]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakDelegate")]
		[Protocolize]
		HMAccessoryDelegate Delegate { get; set; }

		[Export ("reachable")]
		bool Reachable { [Bind ("isReachable")] get; }

		[Export ("bridged")]
		bool Bridged { [Bind ("isBridged")] get; }

		[NoWatch]
		[Availability (Introduced = Platform.iOS_8_0, Deprecated = Platform.iOS_9_0)]
		[Export ("identifiersForBridgedAccessories", ArgumentSemantic.Copy)]
		NSUuid [] IdentifiersForBridgedAccessories { get; }

		[iOS (9,0)]
		[NullAllowed, Export ("uniqueIdentifiersForBridgedAccessories", ArgumentSemantic.Copy)]
		NSUuid[] UniqueIdentifiersForBridgedAccessories { get; }

		[Export ("room", ArgumentSemantic.Weak)]
		HMRoom Room { get; }

		[Export ("services", ArgumentSemantic.Copy)]
		HMService [] Services { get; }

		[Export ("blocked")]
		bool Blocked { [Bind ("isBlocked")] get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[Async]
		[Export ("identifyWithCompletionHandler:")]
		void Identify (Action<NSError> completion);

		[iOS (9,0)]
		[Export ("category", ArgumentSemantic.Strong)]
		HMAccessoryCategory Category { get; }
	}

	[iOS (8,0)]
	[Model, Protocol]
	[BaseType (typeof (NSObject))]
	public partial interface HMAccessoryDelegate {

		[Export ("accessoryDidUpdateName:")]
		void DidUpdateName (HMAccessory accessory);

		[Export ("accessory:didUpdateNameForService:"), EventArgs ("HMAccessoryUpdate")]
		void DidUpdateNameForService (HMAccessory accessory, HMService service);

		[Export ("accessory:didUpdateAssociatedServiceTypeForService:"), EventArgs ("HMAccessoryUpdate")]
		void DidUpdateAssociatedServiceType (HMAccessory accessory, HMService service);

		[Export ("accessoryDidUpdateServices:")]
		void DidUpdateServices (HMAccessory accessory);

		[Export ("accessoryDidUpdateReachability:")]
		void DidUpdateReachability (HMAccessory accessory);

		[Export ("accessory:service:didUpdateValueForCharacteristic:"), EventArgs ("HMAccessoryServiceUpdateCharacteristic")]
		void DidUpdateValueForCharacteristic (HMAccessory accessory, HMService service, HMCharacteristic characteristic);
	}

#if !WATCH
	// __WATCHOS_PROHIBITED
	[iOS (8,0)]
	[BaseType (typeof (NSObject), Delegates=new string[] {"WeakDelegate"}, Events=new Type[] {typeof(HMAccessoryBrowserDelegate)})]
	public partial interface HMAccessoryBrowser {

		[Export ("delegate", ArgumentSemantic.Weak)]
		[NullAllowed]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakDelegate")]
		[Protocolize]
		HMAccessoryBrowserDelegate Delegate { get; set; }

		[Export ("discoveredAccessories", ArgumentSemantic.Copy)]
		HMAccessory [] DiscoveredAccessories { get; }

		[Export ("startSearchingForNewAccessories")]
		void StartSearchingForNewAccessories ();

		[Export ("stopSearchingForNewAccessories")]
		void StopSearchingForNewAccessories ();
	}

	[iOS (8,0)]
	[Model, Protocol]
	[BaseType (typeof (NSObject))]
	public partial interface HMAccessoryBrowserDelegate {

		[Export ("accessoryBrowser:didFindNewAccessory:"), EventArgs ("HMAccessoryBrowser")]
		void DidFindNewAccessory (HMAccessoryBrowser browser, HMAccessory accessory);

		[Export ("accessoryBrowser:didRemoveNewAccessory:"), EventArgs ("HMAccessoryBrowser")]
		void DidRemoveNewAccessory (HMAccessoryBrowser browser, HMAccessory accessory);
	}
#endif // !WATCH

	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	public partial interface HMAction {

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (NSObject))]
	public partial interface HMActionSet {

		[Export ("name")]
		string Name { get; }

		[Export ("actions", ArgumentSemantic.Copy)]
		NSSet Actions { get; }

		[Export ("executing")]
		bool Executing { [Bind ("isExecuting")] get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("addAction:completionHandler:")]
		void AddAction (HMAction action, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeAction:completionHandler:")]
		void RemoveAction (HMAction action, Action<NSError> completion);

		[Internal]
		[iOS (9,0)]
		[Export ("actionSetType")]
		NSString _ActionSetType { get; }

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (9,0)]
	[Static]
	[Internal]
	public interface HMActionSetTypesInternal {
		[Field ("HMActionSetTypeWakeUp")]
		NSString WakeUp { get; }

		[Field ("HMActionSetTypeSleep")]
		NSString Sleep { get; }

		[Field ("HMActionSetTypeHomeDeparture")]
		NSString HomeDeparture { get; }

		[Field ("HMActionSetTypeHomeArrival")]
		NSString HomeArrival { get; }

		[Field ("HMActionSetTypeUserDefined")]
		NSString UserDefined { get; }
	}

	[iOS (8,0)]	
	[BaseType (typeof (NSObject))]
	public partial interface HMCharacteristic {

		[Internal]
		[Export ("characteristicType", ArgumentSemantic.Copy)]
		NSString _CharacteristicType { get; }

		[Export ("service", ArgumentSemantic.Weak)]
		HMService Service { get; }

		[Export ("properties", ArgumentSemantic.Copy)]
		NSString [] Properties { get; }

		[Export ("metadata", ArgumentSemantic.Retain)]
		HMCharacteristicMetadata Metadata { get; }

		[Export ("value", ArgumentSemantic.Copy)]
		NSObject Value { get; }

		[Export ("notificationEnabled")]
		bool NotificationEnabled { [Bind ("isNotificationEnabled")] get; }

		[Async]
		[Export ("writeValue:completionHandler:")]
		void WriteValue (NSObject value, Action<NSError> completion);

		[Async]
		[Export ("readValueWithCompletionHandler:")]
		void ReadValue (Action<NSError> completion);

		[Async]
		[Export ("enableNotification:completionHandler:")]
		void EnableNotification (bool enable, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("updateAuthorizationData:completionHandler:")]
		void UpdateAuthorizationData (NSData data, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("localizedDescription")]
		string LocalizedDescription { get; }

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicKeyPath")]
		NSString KeyPath { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicValueKeyPath")]
		NSString ValueKeyPath { get; }
	}

	[iOS(8,0)]
	[Static]
	[Internal]
	public interface HMCharacteristicPropertyInternal {

		[Field ("HMCharacteristicPropertyReadable")]
		NSString Readable { get; }

		[Field ("HMCharacteristicPropertyWritable")]
		NSString Writable { get; }

		[iOS (9,3)][Watch (2,2)]
		[Field ("HMCharacteristicPropertyHidden")]
		NSString Hidden { get; }

		[Field ("HMCharacteristicPropertySupportsEventNotification")]
		NSString SupportsEventNotification { get; }		
	}

	[iOS(8,0)]
	[Static]
	[Internal]
	public interface HMCharacteristicTypeInternal {
		[Field ("HMCharacteristicTypePowerState")]
		NSString PowerState { get; }

		[Field ("HMCharacteristicTypeHue")]
		NSString Hue { get; }

		[Field ("HMCharacteristicTypeSaturation")]
		NSString Saturation { get; }

		[Field ("HMCharacteristicTypeBrightness")]
		NSString Brightness { get; }

		[Field ("HMCharacteristicTypeTemperatureUnits")]
		NSString TemperatureUnits { get; }

		[Field ("HMCharacteristicTypeCurrentTemperature")]
		NSString CurrentTemperature { get; }

		[Field ("HMCharacteristicTypeTargetTemperature")]
		NSString TargetTemperature { get; }

		[Field ("HMCharacteristicTypeCurrentHeatingCooling")]
		NSString CurrentHeatingCooling { get; }

		[Field ("HMCharacteristicTypeTargetHeatingCooling")]
		NSString TargetHeatingCooling { get; }

		[Field ("HMCharacteristicTypeCoolingThreshold")]
		NSString CoolingThreshold { get; }

		[Field ("HMCharacteristicTypeHeatingThreshold")]
		NSString HeatingThreshold { get; }

		[Field ("HMCharacteristicTypeCurrentRelativeHumidity")]
		NSString CurrentRelativeHumidity { get; }

		[Field ("HMCharacteristicTypeTargetRelativeHumidity")]
		NSString TargetRelativeHumidity { get; }

		[Field ("HMCharacteristicTypeCurrentDoorState")]
		NSString CurrentDoorState { get; }

		[Field ("HMCharacteristicTypeTargetDoorState")]
		NSString TargetDoorState { get; }

		[Field ("HMCharacteristicTypeObstructionDetected")]
		NSString ObstructionDetected { get; }

		[Field ("HMCharacteristicTypeName")]
		NSString Name { get; }

		[Field ("HMCharacteristicTypeManufacturer")]
		NSString Manufacturer { get; }

		[Field ("HMCharacteristicTypeModel")]
		NSString Model { get; }

		[Field ("HMCharacteristicTypeSerialNumber")]
		NSString SerialNumber { get; }

		[Field ("HMCharacteristicTypeIdentify")]
		NSString Identify { get; }

		[Field ("HMCharacteristicTypeRotationDirection")]
		NSString RotationDirection { get; }

		[Field ("HMCharacteristicTypeRotationSpeed")]
		NSString RotationSpeed { get; }

		[Field ("HMCharacteristicTypeOutletInUse")]
		NSString OutletInUse { get; }

		[Field ("HMCharacteristicTypeVersion")]
		NSString Version { get; }

		[Field ("HMCharacteristicTypeLogs")]
		NSString Logs { get; }

		[Field ("HMCharacteristicTypeAudioFeedback")]
		NSString AudioFeedback { get; }

		[Field ("HMCharacteristicTypeAdminOnlyAccess")]
		NSString AdminOnlyAccess { get; }

		[Field ("HMCharacteristicTypeMotionDetected")]
		NSString MotionDetected { get; }

		[Field ("HMCharacteristicTypeCurrentLockMechanismState")]
		NSString CurrentLockMechanismState { get; }

		[Field ("HMCharacteristicTypeTargetLockMechanismState")]
		NSString TargetLockMechanismState { get; }

		[Field ("HMCharacteristicTypeLockMechanismLastKnownAction")]
		NSString LockMechanismLastKnownAction { get; }

		[Field ("HMCharacteristicTypeLockManagementControlPoint")]
		NSString LockManagementControlPoint { get; }

		[Field ("HMCharacteristicTypeLockManagementAutoSecureTimeout")]
		NSString LockManagementAutoSecureTimeout { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeAirParticulateDensity")]
		NSString AirParticulateDensity { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeAirParticulateSize")]
		NSString AirParticulateSize { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeBatteryLevel")]
		NSString BatteryLevel { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCarbonMonoxideDetected")]
		NSString CarbonMonoxideDetected { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeContactState")]
		NSString ContactState { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCurrentHorizontalTilt")]
		NSString CurrentHorizontalTilt { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCurrentLightLevel")]
		NSString CurrentLightLevel { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCurrentPosition")]
		NSString CurrentPosition { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCurrentVerticalTilt")]
		NSString CurrentVerticalTilt { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeFirmwareVersion")]
		NSString FirmwareVersion { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeHardwareVersion")]
		NSString HardwareVersion { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeHoldPosition")]
		NSString HoldPosition { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeInputEvent")]
		NSString InputEvent { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeLeakDetected")]
		NSString LeakDetected { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeOccupancyDetected")]
		NSString OccupancyDetected { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeOutputState")]
		NSString OutputState { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypePositionState")]
		NSString PositionState { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeSmokeDetected")]
		NSString SmokeDetected { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeSoftwareVersion")]
		NSString SoftwareVersion { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeStatusActive")]
		NSString StatusActive { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeStatusFault")]
		NSString StatusFault { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeStatusJammed")]
		NSString StatusJammed { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeStatusLowBattery")]
		NSString StatusLowBattery { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeStatusTampered")]
		NSString StatusTampered { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeTargetHorizontalTilt")]
		NSString TargetHorizontalTilt { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeTargetPosition")]
		NSString TargetPosition { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeTargetVerticalTilt")]
		NSString TargetVerticalTilt { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeSecuritySystemAlarmType")]
		NSString SecuritySystemAlarmType { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeAirQuality")]
		NSString AirQuality { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCarbonDioxideDetected")]
		NSString CarbonDioxideDetected { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCarbonDioxideLevel")]
		NSString CarbonDioxideLevel { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCarbonDioxidePeakLevel")]
		NSString CarbonDioxidePeakLevel { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCarbonMonoxideLevel")]
		NSString CarbonMonoxideLevel { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCarbonMonoxidePeakLevel")]
		NSString CarbonMonoxidePeakLevel { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeChargingState")]
		NSString ChargingState { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeCurrentSecuritySystemState")]
		NSString CurrentSecuritySystemState { get; }

		[iOS (9,0)]
		[Field ("HMCharacteristicTypeTargetSecuritySystemState")]
		NSString TargetSecuritySystemState { get; }
	}

	[iOS (8,0)]
	[Static]
	[Internal]
	interface HMCharacteristicMetadataUnitsInternal {
		[Field ("HMCharacteristicMetadataUnitsCelsius")]
		NSString Celsius { get; }

		[Field ("HMCharacteristicMetadataUnitsFahrenheit")]
		NSString Fahrenheit { get; }

		[Field ("HMCharacteristicMetadataUnitsPercentage")]
		NSString Percentage { get; }

		[Field ("HMCharacteristicMetadataUnitsArcDegree")]
		NSString ArcDegree { get; }

		[iOS (8,3)]
		[Field ("HMCharacteristicMetadataUnitsSeconds")]
		NSString Seconds { get; }

		[iOS (9,3)][Watch (2,2)]
		[Field ("HMCharacteristicMetadataUnitsLux")]
		NSString Lux { get; }
	}

	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	public partial interface HMCharacteristicMetadata {

		[Export ("minimumValue")]
		NSNumber MinimumValue { get; }

		[Export ("maximumValue")]
		NSNumber MaximumValue { get; }

		[Export ("stepValue")]
		NSNumber StepValue { get; }

		[Export ("maxLength")]
		NSNumber MaxLength { get; }

		[Internal]
		[Export ("format", ArgumentSemantic.Copy)]
		NSString _Format { get; }

		[Internal]
		[Export ("units", ArgumentSemantic.Copy)]
		NSString _Units { get; }

		[Export ("manufacturerDescription")]
		string ManufacturerDescription { get; }
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (HMAction))]
	public partial interface HMCharacteristicWriteAction {

		[NoWatch]
		[DesignatedInitializer]
		[Export ("initWithCharacteristic:targetValue:")]
#if XAMCORE_3_0
		IntPtr Constructor (HMCharacteristic characteristic, INSCopying targetValue);
#else
		IntPtr Constructor (HMCharacteristic characteristic, NSObject targetValue);
#endif

		[Export ("characteristic", ArgumentSemantic.Retain)]
		HMCharacteristic Characteristic { get; }

		[Export ("targetValue", ArgumentSemantic.Copy)]
#if XAMCORE_3_0
		INSCopying TargetValue { get; }
#else
		NSObject TargetValue { get; }
#endif

		[NoWatch]
		[Async]
		[Export ("updateTargetValue:completionHandler:")]
#if XAMCORE_3_0
		void UpdateTargetValue (INSCopying targetValue, Action<NSError> completion);
#else
		void UpdateTargetValue (NSObject targetValue, Action<NSError> completion);
#endif
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (NSObject), Delegates=new string[] {"WeakDelegate"}, Events=new Type[] {typeof(HMHomeDelegate)})]
	public partial interface HMHome { 

		[Export ("delegate", ArgumentSemantic.Weak)]
		[NullAllowed]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakDelegate")]
		[Protocolize]
		HMHomeDelegate Delegate { get; set; }

		[Export ("name")]
		string Name { get; }

		[Export ("primary")]
		bool Primary { [Bind ("isPrimary")] get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }

		// HMHome(HMAccessory)

		[Export ("accessories", ArgumentSemantic.Copy)]
		HMAccessory [] Accessories { get; }

		[NoWatch]
		[Async]
		[Export ("addAccessory:completionHandler:")]
		void AddAccessory (HMAccessory accessory, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeAccessory:completionHandler:")]
		void RemoveAccessory (HMAccessory accessory, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("assignAccessory:toRoom:completionHandler:")]
		void AssignAccessory (HMAccessory accessory, HMRoom room, Action<NSError> completion);

		[Internal]
		[Export ("servicesWithTypes:")]
		HMService [] _ServicesWithTypes (NSString [] serviceTypes);

		[NoWatch]
		[Async]
		[Export ("unblockAccessory:completionHandler:")]
		void UnblockAccessory (HMAccessory accessory, Action<NSError> completion);

		// HMHome(HMRoom)

		[Export ("rooms", ArgumentSemantic.Copy)]
		HMRoom [] Rooms { get; }

		[NoWatch]
		[Async]
		[Export ("addRoomWithName:completionHandler:")]
		void AddRoom (string roomName, Action<HMRoom, NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeRoom:completionHandler:")]
		void RemoveRoom (HMRoom room, Action<NSError> completion);

		[Export ("roomForEntireHome")]
		HMRoom GetRoomForEntireHome ();

		// HMHome(HMZone)

		[Export ("zones", ArgumentSemantic.Copy)]
		HMZone [] Zones { get; }

		[NoWatch]
		[Async]
		[Export ("addZoneWithName:completionHandler:")]
		void AddZone (string zoneName, Action<HMZone, NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeZone:completionHandler:")]
		void RemoveZone (HMZone zone, Action<NSError> completion);

		// HMHome(HMServiceGroup)

		[Export ("serviceGroups", ArgumentSemantic.Copy)]
		HMServiceGroup [] ServiceGroups { get; }

		[NoWatch]
		[Async]
		[Export ("addServiceGroupWithName:completionHandler:")]
		void AddServiceGroup (string serviceGroupName, Action<HMServiceGroup, NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeServiceGroup:completionHandler:")]
		void RemoveServiceGroup (HMServiceGroup group, Action<NSError> completion);

		// HMHome(HMActionSet)

		[Export ("actionSets", ArgumentSemantic.Copy)]
		HMActionSet [] ActionSets { get; }

		[NoWatch]
		[Async]
		[Export ("addActionSetWithName:completionHandler:")]
		void AddActionSet (string actionSetName, Action<HMActionSet, NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeActionSet:completionHandler:")]
		void RemoveActionSet (HMActionSet actionSet, Action<NSError> completion);

		[Async]
		[Export ("executeActionSet:completionHandler:")]
		void ExecuteActionSet (HMActionSet actionSet, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("builtinActionSetOfType:")]
		[return: NullAllowed]
		HMActionSet GetBuiltinActionSet (string actionSetType);

		// HMHome(HMTrigger)

		[Export ("triggers", ArgumentSemantic.Copy)]
		HMTrigger [] Triggers { get; }

		[NoWatch]
		[Async]
		[Export ("addTrigger:completionHandler:")]
		void AddTrigger (HMTrigger trigger, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeTrigger:completionHandler:")]
		void RemoveTrigger (HMTrigger trigger, Action<NSError> completion);

		// HMHome(HMUser)

		[NoWatch]
		[Availability (Introduced = Platform.iOS_8_0, Deprecated = Platform.iOS_9_0)]
		[Export ("users")]
		HMUser [] Users { get; }

		[NoWatch]
		[Availability (Introduced = Platform.iOS_8_0, Deprecated = Platform.iOS_9_0)]
		[Async]
		[Export ("addUserWithCompletionHandler:")]
		void AddUser (Action<HMUser,NSError> completion);

		[NoWatch]
		[Availability (Introduced = Platform.iOS_8_0, Deprecated = Platform.iOS_9_0)]
		[Async]
		[Export ("removeUser:completionHandler:")]
		void RemoveUser (HMUser user, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("currentUser", ArgumentSemantic.Strong)]
		HMUser CurrentUser { get; }

		[NoWatch]
		[iOS (9,0)]
		[Export ("manageUsersWithCompletionHandler:")]
		void ManageUsers (Action<NSError> completion);

		[iOS (9,0)]
		[Export ("homeAccessControlForUser:")]
		HMHomeAccessControl GetHomeAccessControl (HMUser user);

		// constants

		[Field ("HMUserFailedAccessoriesKey")]
		NSString UserFailedAccessoriesKey { get; }
	}

	[iOS (8,0)]
	[Model, Protocol]
	[BaseType (typeof (NSObject))]
	public partial interface HMHomeDelegate {

		[Export ("homeDidUpdateName:")]
		void DidUpdateNameForHome (HMHome home);

		[Export ("home:didAddAccessory:"), EventArgs ("HMHomeAccessory")]
		void DidAddAccessory (HMHome home, HMAccessory accessory);

		[Export ("home:didRemoveAccessory:"), EventArgs ("HMHomeAccessory")]
		void DidRemoveAccessory (HMHome home, HMAccessory accessory);

		[Export ("home:didAddUser:"), EventArgs ("HMHomeUser")]
		void DidAddUser (HMHome home, HMUser user);

		[Export ("home:didRemoveUser:"), EventArgs ("HMHomeUser")]
		void DidRemoveUser (HMHome home, HMUser user);

		[Export ("home:didUpdateRoom:forAccessory:"), EventArgs ("HMHomeRoomAccessory")]
		void DidUpdateRoom (HMHome home, HMRoom room, HMAccessory accessory);

		[Export ("home:didAddRoom:"), EventArgs ("HMHomeRoom")]
		void DidAddRoom (HMHome home, HMRoom room);

		[Export ("home:didRemoveRoom:"), EventArgs ("HMHomeRoom")]
		void DidRemoveRoom (HMHome home, HMRoom room);

		[Export ("home:didUpdateNameForRoom:"), EventArgs ("HMHomeRoom")]
		void DidUpdateNameForRoom (HMHome home, HMRoom room);

		[Export ("home:didAddZone:"), EventArgs ("HMHomeZone")]
		void DidAddZone (HMHome home, HMZone zone);

		[Export ("home:didRemoveZone:"), EventArgs ("HMHomeZone")]
		void DidRemoveZone (HMHome home, HMZone zone);

		[Export ("home:didUpdateNameForZone:"), EventArgs ("HMHomeZone")]
		void DidUpdateNameForZone (HMHome home, HMZone zone);

		[Export ("home:didAddRoom:toZone:"), EventArgs ("HMHomeRoomZone")]
		void DidAddRoomToZone (HMHome home, HMRoom room, HMZone zone);

		[Export ("home:didRemoveRoom:fromZone:"), EventArgs ("HMHomeRoomZone")]
		void DidRemoveRoomFromZone (HMHome home, HMRoom room, HMZone zone);

		[Export ("home:didAddServiceGroup:"), EventArgs ("HMHomeServiceGroup")]
		void DidAddServiceGroup (HMHome home, HMServiceGroup group);

		[Export ("home:didRemoveServiceGroup:"), EventArgs ("HMHomeServiceGroup")]
		void DidRemoveServiceGroup (HMHome home, HMServiceGroup group);

		[Export ("home:didUpdateNameForServiceGroup:"), EventArgs ("HMHomeServiceGroup")]
		void DidUpdateNameForServiceGroup (HMHome home, HMServiceGroup group);

		[Export ("home:didAddService:toServiceGroup:"), EventArgs ("HMHomeServiceServiceGroup")]
		void DidAddService (HMHome home, HMService service, HMServiceGroup group);

		[Export ("home:didRemoveService:fromServiceGroup:"), EventArgs ("HMHomeServiceServiceGroup")]
		void DidRemoveService (HMHome home, HMService service, HMServiceGroup group);

		[Export ("home:didAddActionSet:"), EventArgs ("HMHomeActionSet")]
		void DidAddActionSet (HMHome home, HMActionSet actionSet);

		[Export ("home:didRemoveActionSet:"), EventArgs ("HMHomeActionSet")]
		void DidRemoveActionSet (HMHome home, HMActionSet actionSet);

		[Export ("home:didUpdateNameForActionSet:"), EventArgs ("HMHomeActionSet")]
		void DidUpdateNameForActionSet (HMHome home, HMActionSet actionSet);

		[Export ("home:didUpdateActionsForActionSet:"), EventArgs ("HMHomeActionSet")]
		void DidUpdateActionsForActionSet (HMHome home, HMActionSet actionSet);

		[Export ("home:didAddTrigger:"), EventArgs ("HMHomeTrigger")]
		void DidAddTrigger (HMHome home, HMTrigger trigger);

		[Export ("home:didRemoveTrigger:"), EventArgs ("HMHomeTrigger")]
		void DidRemoveTrigger (HMHome home, HMTrigger trigger);

		[Export ("home:didUpdateNameForTrigger:"), EventArgs ("HMHomeTrigger")]
		void DidUpdateNameForTrigger (HMHome home, HMTrigger trigger);

		[Export ("home:didUpdateTrigger:"), EventArgs ("HMHomeTrigger")]
		void DidUpdateTrigger (HMHome home, HMTrigger trigger);

		[Export ("home:didUnblockAccessory:"), EventArgs ("HMHomeAccessory")]
		void DidUnblockAccessory (HMHome home, HMAccessory accessory);

		[Export ("home:didEncounterError:forAccessory:"), EventArgs ("HMHomeErrorAccessory")]
		void DidEncounterError (HMHome home, NSError error, HMAccessory accessory);
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (NSObject))]
	public partial interface HMRoom {

		[Export ("name")]
		string Name { get; }

		[Export ("accessories", ArgumentSemantic.Copy)]
		HMAccessory [] Accessories { get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (8,0)]
	[Static]
	[Internal]
	public interface HMServiceTypeInternal {
		[Field ("HMServiceTypeLightbulb")]
		NSString LightBulb { get; }

		[Field ("HMServiceTypeSwitch")]
		NSString Switch { get; }

		[Field ("HMServiceTypeThermostat")]
		NSString Thermostat { get; }

		[Field ("HMServiceTypeGarageDoorOpener")]
		NSString GarageDoorOpener { get; }

		[Field ("HMServiceTypeAccessoryInformation")]
		NSString AccessoryInformation { get; }

		[Field ("HMServiceTypeFan")]
		NSString Fan { get; }

		[Field ("HMServiceTypeOutlet")]
		NSString Outlet { get; }

		[Field ("HMServiceTypeLockMechanism")]
		NSString LockMechanism { get; }

		[Field ("HMServiceTypeLockManagement")]
		NSString LockManagement { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeAirQualitySensor")]
		NSString AirQualitySensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeCarbonMonoxideSensor")]
		NSString CarbonMonoxideSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeContactSensor")]
		NSString ContactSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeDoor")]
		NSString Door { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeHumiditySensor")]
		NSString HumiditySensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeLeakSensor")]
		NSString LeakSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeLightSensor")]
		NSString LightSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeMotionSensor")]
		NSString MotionSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeOccupancySensor")]
		NSString OccupancySensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeStatefulProgrammableSwitch")]
		NSString StatefulProgrammableSwitch { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeStatelessProgrammableSwitch")]
		NSString StatelessProgrammableSwitch { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeSmokeSensor")]
		NSString SmokeSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeTemperatureSensor")]
		NSString TemperatureSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeWindow")]
		NSString Window { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeWindowCovering")]
		NSString WindowCovering { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeBattery")]
		NSString Battery { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeCarbonDioxideSensor")]
		NSString CarbonDioxideSensor { get; }

		[iOS (9,0)]
		[Field ("HMServiceTypeSecuritySystem")]
		NSString SecuritySystem { get; }
	}

	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	public partial interface HMService { 

		[Export ("accessory", ArgumentSemantic.Weak)]
		HMAccessory Accessory { get; }

		[Internal]
		[Export ("serviceType", ArgumentSemantic.Copy)]
		NSString _ServiceType { get; }

		[Export ("name")]
		string Name { get; }

		[Export ("associatedServiceType")]
		string AssociatedServiceType { get; }

		[Export ("characteristics", ArgumentSemantic.Copy)]
		HMCharacteristic [] Characteristics { get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[NoWatch]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Async]
		[Export ("updateAssociatedServiceType:completionHandler:")]
		void UpdateAssociatedServiceType ([NullAllowed] string serviceType, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("userInteractive")]
		bool UserInteractive { [Bind ("isUserInteractive")] get; }

		[iOS (9,0)]
		[Export ("localizedDescription")]
		string LocalizedDescription { get; }

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (NSObject))]
	public partial interface HMServiceGroup {

		[Export ("name")]
		string Name { get; }

		[Export ("services", ArgumentSemantic.Copy)]
		HMService [] Services { get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("addService:completionHandler:")]
		void AddService (HMService service, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeService:completionHandler:")]
		void RemoveService (HMService service, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (HMTrigger))]
	public partial interface HMTimerTrigger { 

		[NoWatch]
		[DesignatedInitializer]
		[Export ("initWithName:fireDate:timeZone:recurrence:recurrenceCalendar:")]
		IntPtr Constructor (string name, NSDate fireDate, [NullAllowed] NSTimeZone timeZone, [NullAllowed] NSDateComponents recurrence, [NullAllowed] NSCalendar recurrenceCalendar);

		[Export ("fireDate", ArgumentSemantic.Copy)]
		NSDate FireDate { get; }

		[Export ("timeZone", ArgumentSemantic.Copy)]
		NSTimeZone TimeZone { get; }

		[Export ("recurrence", ArgumentSemantic.Copy)]
		NSDateComponents Recurrence { get; }

		[Export ("recurrenceCalendar", ArgumentSemantic.Copy)]
		NSCalendar RecurrenceCalendar { get; }

		[NoWatch]
		[Async]
		[Export ("updateFireDate:completionHandler:")]
		void UpdateFireDate (NSDate fireDate, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("updateTimeZone:completionHandler:")]
		void UpdateTimeZone ([NullAllowed] NSTimeZone timeZone, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("updateRecurrence:completionHandler:")]
		void UpdateRecurrence ([NullAllowed] NSDateComponents recurrence, Action<NSError> completion);
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (NSObject))]
	public partial interface HMTrigger { 

		[Export ("name")]
		string Name { get; }

		[Export ("enabled")]
		bool Enabled { [Bind ("isEnabled")] get; }

		[Export ("actionSets", ArgumentSemantic.Copy)]
		HMActionSet [] ActionSets { get; }

		[Export ("lastFireDate", ArgumentSemantic.Copy)]
		NSDate LastFireDate { get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("addActionSet:completionHandler:")]
		void AddActionSet (HMActionSet actionSet, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeActionSet:completionHandler:")]
		void RemoveActionSet (HMActionSet actionSet, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("enable:completionHandler:")]
		void Enable (bool enable, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (8,0)]
	[DisableDefaultCtor]
	[BaseType (typeof (NSObject))]
	public partial interface HMZone { 

		[Export ("name")]
		string Name { get; }

		[Export ("rooms", ArgumentSemantic.Copy)]
		HMRoom [] Rooms { get; }

		[NoWatch]
		[Async]
		[Export ("updateName:completionHandler:")]
		void UpdateName (string name, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("addRoom:completionHandler:")]
		void AddRoom (HMRoom room, Action<NSError> completion);

		[NoWatch]
		[Async]
		[Export ("removeRoom:completionHandler:")]
		void RemoveRoom (HMRoom room, Action<NSError> completion);

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[Static, Internal]
	[iOS (8,0)]
	interface HMCharacteristicMetadataFormatKeys {
		[Field ("HMCharacteristicMetadataFormatBool")]
		NSString _Bool { get; }

		[Field ("HMCharacteristicMetadataFormatInt")]
		NSString _Int { get; }

		[Field ("HMCharacteristicMetadataFormatFloat")]
		NSString _Float { get; }

		[Field ("HMCharacteristicMetadataFormatString")]
		NSString _String { get; }
		
		[Field ("HMCharacteristicMetadataFormatArray")]
		NSString _Array { get; }

		[Field ("HMCharacteristicMetadataFormatDictionary")]
		NSString _Dictionary { get; }

		[Field ("HMCharacteristicMetadataFormatUInt8")]
		NSString _UInt8 { get; }

		[Field ("HMCharacteristicMetadataFormatUInt16")]
		NSString _UInt16 { get; }

		[Field ("HMCharacteristicMetadataFormatUInt32")]
		NSString _UInt32 { get; }

		[Field ("HMCharacteristicMetadataFormatUInt64")]
		NSString _UInt64 { get; }

		[Field ("HMCharacteristicMetadataFormatData")]
		NSString _Data { get; }

		[Field ("HMCharacteristicMetadataFormatTLV8")]
		NSString _Tlv8 { get; }
	}

	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor]
	public interface HMUser {
		[Export ("name")]
		string Name { get; }

		[iOS (9,0)]
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (9,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor] // NSInternalInconsistencyException Reason: init is unavailable
	public interface HMAccessoryCategory {
		[Internal]
		[Export ("categoryType")]
		NSString _CategoryType { get; }

		[Export ("localizedDescription")]
		string LocalizedDescription { get; }
	}

	[iOS (9,0)]
	[Static]
	[Internal]
	public interface HMAccessoryCategoryTypesInternal {
		[Field ("HMAccessoryCategoryTypeOther")]
		NSString Other { get; }

		[Field ("HMAccessoryCategoryTypeSecuritySystem")]
		NSString SecuritySystem { get; }

		[Field ("HMAccessoryCategoryTypeBridge")]
		NSString Bridge { get; }

		[Field ("HMAccessoryCategoryTypeDoor")]
		NSString Door { get; }

		[Field ("HMAccessoryCategoryTypeDoorLock")]
		NSString DoorLock { get; }

		[Field ("HMAccessoryCategoryTypeFan")]
		NSString Fan { get; }

		[Field ("HMAccessoryCategoryTypeGarageDoorOpener")]
		NSString DoorOpener { get; }

		[Field ("HMAccessoryCategoryTypeLightbulb")]
		NSString Lightbulb { get; }

		[Field ("HMAccessoryCategoryTypeOutlet")]
		NSString Outlet { get; }

		[Field ("HMAccessoryCategoryTypeProgrammableSwitch")]
		NSString ProgrammableSwitch { get; }

		[Field ("HMAccessoryCategoryTypeSensor")]
		NSString Sensor { get; }

		[Field ("HMAccessoryCategoryTypeSwitch")]
		NSString Switch { get; }

		[Field ("HMAccessoryCategoryTypeThermostat")]
		NSString Thermostat { get; }

		[Field ("HMAccessoryCategoryTypeWindow")]
		NSString Window { get; }

		[Field ("HMAccessoryCategoryTypeWindowCovering")]
		NSString WindowCovering { get; }

		[iOS (9,3), Watch (2,2)]
		[Field ("HMAccessoryCategoryTypeRangeExtender")]
		NSString RangeExtender { get; }
	}

	[iOS (9,0)]
	[BaseType (typeof (HMEvent))]
	[DisableDefaultCtor]
	interface HMCharacteristicEvent {
		[NoWatch]
		[Export ("initWithCharacteristic:triggerValue:")]
		IntPtr Constructor (HMCharacteristic characteristic, [NullAllowed] INSCopying triggerValue);

		[Export ("characteristic", ArgumentSemantic.Strong)]
		HMCharacteristic Characteristic { get; }

		[NullAllowed]
		[Export ("triggerValue", ArgumentSemantic.Copy)]
		INSCopying TriggerValue { get; }

		[NoWatch]
		[Export ("updateTriggerValue:completionHandler:")]
		void UpdateTriggerValue ([NullAllowed] INSCopying triggerValue, Action<NSError> completion);
	}

	[iOS (9,0)]
	[BaseType (typeof (NSObject))]
	interface HMEvent {
		[Export ("uniqueIdentifier", ArgumentSemantic.Copy)]
		NSUuid UniqueIdentifier { get; }
	}

	[iOS (9,0)]
	[BaseType (typeof (HMTrigger))]
	[DisableDefaultCtor]
	interface HMEventTrigger {
		[NoWatch]
		[Export ("initWithName:events:predicate:")]
		[DesignatedInitializer]
		IntPtr Constructor (string name, HMEvent[] events, [NullAllowed] NSPredicate predicate);

		[Export ("events", ArgumentSemantic.Copy)]
		HMEvent[] Events { get; }

		[NullAllowed, Export ("predicate", ArgumentSemantic.Copy)]
		NSPredicate Predicate { get; }

		[Static][Internal]
		[Export ("predicateForEvaluatingTriggerOccurringBeforeSignificantEvent:applyingOffset:")]
		NSPredicate CreatePredicateForEvaluatingTriggerOccurringBeforeSignificantEvent (NSString significantEvent, [NullAllowed] NSDateComponents offset);

		[Static][Internal]
		[Export ("predicateForEvaluatingTriggerOccurringAfterSignificantEvent:applyingOffset:")]
		NSPredicate CreatePredicateForEvaluatingTriggerOccurringAfterSignificantEvent (NSString significantEvent, [NullAllowed] NSDateComponents offset);

		[Static]
		[Export ("predicateForEvaluatingTriggerOccurringBeforeDateWithComponents:")]
		NSPredicate CreatePredicateForEvaluatingTriggerOccurringBeforeDate (NSDateComponents dateComponents);

		[Static]
		[Export ("predicateForEvaluatingTriggerOccurringOnDateWithComponents:")]
		NSPredicate CreatePredicateForEvaluatingTriggerOccurringOnDate (NSDateComponents dateComponents);

		[Static]
		[Export ("predicateForEvaluatingTriggerOccurringAfterDateWithComponents:")]
		NSPredicate CreatePredicateForEvaluatingTriggerOccurringAfterDate (NSDateComponents dateComponents);

		[Static]
		[Export ("predicateForEvaluatingTriggerWithCharacteristic:relatedBy:toValue:")]
		NSPredicate CreatePredicateForEvaluatingTrigger (HMCharacteristic characteristic, NSPredicateOperatorType operatorType, NSObject value);

		[NoWatch]
		[Export ("addEvent:completionHandler:")]
		void AddEvent (HMEvent @event, Action<NSError> completion);

		[NoWatch]
		[Export ("removeEvent:completionHandler:")]
		void RemoveEvent (HMEvent @event, Action<NSError> completion);

		[NoWatch]
		[Export ("updatePredicate:completionHandler:")]
		void UpdatePredicate ([NullAllowed] NSPredicate predicate, Action<NSError> completion);
	}

	[Static]
	[Internal]
	[iOS (9,0)]
	partial interface HMSignificantEventInternal {
		
		[Field ("HMSignificantEventSunrise")]
		NSString Sunrise { get; }

		[Field ("HMSignificantEventSunset")]
		NSString Sunset { get; }
	}

	[iOS (9,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor]
	public interface HMHomeAccessControl {
		[Export ("administrator")]
		bool Administrator { [Bind ("isAdministrator")] get; }
	}

	[iOS (9,0)]
	[BaseType (typeof (HMEvent))]
	[DisableDefaultCtor]
	interface HMLocationEvent {
		[NoWatch]
		[Export ("initWithRegion:")]
		IntPtr Constructor (CLRegion region);

		[NullAllowed, Export ("region", ArgumentSemantic.Strong)]
		CLRegion Region { get; }

		[NoWatch]
		[Export ("updateRegion:completionHandler:")]
		void UpdateRegion (CLRegion region, Action<NSError> completion);
	}
}
