//
// spritekit.cs: binding for iOS (7+) and Mac (10.9+) SpriteKit framework
//
// Authors:
//   Aaron Bockover (abock@xamarin.com)
//
// Copyright 2013-2015 Xamarin Inc
#if XAMCORE_2_0 || !MONOMAC
using System;
using System.ComponentModel;

using XamCore.AVFoundation;
using XamCore.ObjCRuntime;
using XamCore.Foundation;
using XamCore.CoreFoundation;
using XamCore.CoreGraphics;
using XamCore.CoreImage;
using XamCore.GLKit;
using XamCore.SceneKit;
using XamCore.CoreVideo;
using XamCore.GameplayKit;

using Vector2 = global::OpenTK.Vector2;
using Vector3 = global::OpenTK.Vector3;
using Matrix2 = global::OpenTK.Matrix2;
using Matrix3 = global::OpenTK.Matrix3;
using Matrix4 = global::OpenTK.Matrix4;
using Vector4 = global::OpenTK.Vector4;
using Quaternion = global::OpenTK.Quaternion;
using MathHelper = global::OpenTK.MathHelper;
#if MONOMAC
using XamCore.AppKit;
using UIColor = global::XamCore.AppKit.NSColor;
using UIImage = global::XamCore.AppKit.NSImage;
using UIView = global::XamCore.AppKit.NSView;
using pfloat = System.nfloat;
#else
using XamCore.UIKit;
using UIView = global::XamCore.UIKit.UIView;
using pfloat = System.Single;
#endif

namespace XamCore.SpriteKit {

	delegate void SKNodeChildEnumeratorHandler (SKNode node, out bool stop);
	delegate void SKActionTimingFunction (float /* float, not CGFloat */ time);
	
	[iOS (8,0), Mac (10,10, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	interface SK3DNode {
		[DesignatedInitializer]
		[Export ("initWithViewportSize:")]
		IntPtr Constructor (CGSize viewportSize);

		[Export ("viewportSize")]
		CGSize ViewportSize { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("scnScene", ArgumentSemantic.Retain)]
		SCNScene ScnScene { get; set; }

		[Export ("sceneTime")]
		double SceneTime { get; set; }

		[Export ("playing")]
		bool Playing { [Bind ("isPlaying")] get; set; }

		[Export ("loops")]
		bool Loops { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("pointOfView", ArgumentSemantic.Retain)]
		SCNNode PointOfView { get; set; }

		[Export ("autoenablesDefaultLighting")]
		bool AutoenablesDefaultLighting { get; set; }

		[Static, Export ("nodeWithViewportSize:")]
		SK3DNode FromViewportSize (CGSize viewportSize);

		[Export ("hitTest:options:")]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		SCNHitTestResult [] HitTest (CGPoint thePoint, [NullAllowed] NSDictionary options);

		[Wrap ("HitTest (thePoint, options == null ? null : options.Dictionary)")]
		SCNHitTestResult [] HitTest (CGPoint thePoint, SCNHitTestOptions options);

		[Export ("projectPoint:")]
		/* vector_float3 */
		[MarshalDirective (NativePrefix = "xamarin_simd__", Library = "__Internal")]
		Vector3 ProjectPoint (Vector3 point);

		[Export ("unprojectPoint:")]
		/* vector_float3 */
		[MarshalDirective (NativePrefix = "xamarin_simd__", Library = "__Internal")]
		Vector3 UnprojectPoint (Vector3 point);
	}

#if MONOMAC
	[Mac (10,9, onlyOn64 : true)]
	[BaseType (typeof (NSResponder))]
#else
	[Since (7,0)]
	[BaseType (typeof (UIResponder))]
#endif
	partial interface SKNode : NSCoding, NSCopying {
		[Static, Export ("node")]
		SKNode Create ();

		[Export ("frame")]
		CGRect Frame { get; }

		[Export ("calculateAccumulatedFrame")]
		CGRect CalculateAccumulatedFrame ();

		[Export ("position")]
		CGPoint Position { get; set; }

		[Export ("zPosition")]
		nfloat ZPosition { get; set; }

		[Export ("zRotation")]
		nfloat ZRotation { get; set; }

		[Export ("xScale")]
		nfloat XScale { get; set; }

		[Export ("yScale")]
		nfloat YScale { get; set; }

		[Export ("speed")]
		nfloat Speed { get; set; }

		[Export ("alpha")]
		nfloat Alpha { get; set; }

		[Export ("paused")]
		bool Paused { [Bind ("isPaused")] get; set; }

		[Export ("hidden")]
		bool Hidden { [Bind ("isHidden")] get; set; }

		[Export ("userInteractionEnabled")]
		bool UserInteractionEnabled { [Bind ("isUserInteractionEnabled")] get; set; }

		[Export ("parent")]
		SKNode Parent { get; }

		[Export ("children")]
		SKNode [] Children { get; }

		[NullAllowed] // by default this property is null
		[Export ("name", ArgumentSemantic.Copy)]
		string Name { get; set; }

		[Export ("scene")]
		SKScene Scene { get; }

		[Export ("physicsBody", ArgumentSemantic.Retain), NullAllowed]
		SKPhysicsBody PhysicsBody { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("userData", ArgumentSemantic.Retain)]
		NSMutableDictionary UserData { get; set; }

#if XAMCORE_2_0
		[Export ("setScale:")]
		void SetScale (nfloat scale);
#else
		[Export ("scale")]
		nfloat Scale { set; }
#endif

		[Export ("addChild:")]
		[PostGet ("Children")]
		void AddChild (SKNode node);

		[Export ("insertChild:atIndex:")]
		[PostGet ("Children")]
		void InsertChild (SKNode node, nint index);

		[Export ("removeChildrenInArray:")]
		[PostGet ("Children")]
		void RemoveChildren (SKNode [] nodes);

		[Export ("removeAllChildren")]
		[PostGet ("Children")]
		void RemoveAllChildren ();

		[Export ("removeFromParent")]
		void RemoveFromParent ();

		[Export ("childNodeWithName:")]
		SKNode GetChildNode (string name);

		[Export ("enumerateChildNodesWithName:usingBlock:")]
		void EnumerateChildNodes (string name, SKNodeChildEnumeratorHandler enumerationHandler);

		[Export ("runAction:")]
		void RunAction (SKAction action);

		[Export ("runAction:completion:")]
		void RunAction (SKAction action, Action completionHandler);

		[Export ("runAction:withKey:")]
		void RunAction (SKAction action, string key);

		[Export ("hasActions")]
		bool HasActions { get; }

		[Export ("actionForKey:")]
		SKAction GetActionForKey (string key);

		[Export ("removeActionForKey:")]
		void RemoveActionForKey (string key);

		[Export ("removeAllActions")]
		void RemoveAllActions ();

		[Export ("containsPoint:")]
		bool ContainsPoint (CGPoint point);

		[Export ("nodeAtPoint:")]
		SKNode GetNodeAtPoint (CGPoint point);

		[Export ("nodesAtPoint:")]
		SKNode [] GetNodesAtPoint (CGPoint point);

		[Export ("convertPoint:fromNode:")]
		CGPoint ConvertPointFromNode (CGPoint point, SKNode sourceNode);

		[Export ("convertPoint:toNode:")]
		CGPoint ConvertPointToNode (CGPoint point, SKNode sourceNode);

		[Export ("intersectsNode:")]
		bool IntersectsNode (SKNode node);

		[Export ("inParentHierarchy:")]
		bool InParentHierarchy (SKNode node);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[NullAllowed] // by default this property is null
		[Export ("reachConstraints", ArgumentSemantic.Copy)]
		SKReachConstraints ReachConstraints { get; set; }

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[NullAllowed] // by default this property is null
		[Export ("constraints", ArgumentSemantic.Copy)]
		SKConstraint [] Constraints { get; set; }

		[iOS (8,0),Mac(10,10)]
		[Export ("objectForKeyedSubscript:")]
		SKNode GetObjectsMatching (string nameExpression);

		[iOS (9,0),Mac(10,11)]
		[Export ("moveToParent:")]
		void MoveToParent (SKNode parent);

		[iOS (9,0),Mac(10,11)]
		[Static]
		[Export ("obstaclesFromNodeBounds:")]
		GKPolygonObstacle[] ObstaclesFromNodeBounds (SKNode[] nodes);
		
		[iOS (9,0),Mac(10,11)]
		[Static]
		[Export ("obstaclesFromNodePhysicsBodies:")]
		GKPolygonObstacle[] ObstaclesFromNodePhysicsBodies (SKNode[] nodes);

		[iOS (9,0),Mac(10,11)]
		[Static]
		[Export ("obstaclesFromSpriteTextures:accuracy:")]
		GKPolygonObstacle[] ObstaclesFromSpriteTextures (SKNode[] sprites, float accuracy);
	}

#if MONOMAC
	[Mac(10,9, onlyOn64 : true)]
	[Category, BaseType (typeof (NSEvent))]
	partial interface SKNodeEvent_NSEvent {

		[Export ("locationInNode:")]
		CGPoint LocationInNode (SKNode node);
	}
#else
	[Since (7,0)]
	[Category, BaseType (typeof (UITouch))]
	partial interface SKNodeTouches_UITouch {

		[Export ("locationInNode:")]
		CGPoint LocationInNode (SKNode node);

		[Export ("previousLocationInNode:")]
		CGPoint PreviousLocationInNode (SKNode node);
	}
#endif

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKNode))]
	partial interface SKEffectNode {

		[NullAllowed] // by default this property is null
		[Export ("filter", ArgumentSemantic.Retain)]
		CIFilter Filter { get; set; }

		[Export ("shouldCenterFilter")]
		bool ShouldCenterFilter { get; set; }

		[Export ("shouldEnableEffects")]
		bool ShouldEnableEffects { get; set; }

		[Export ("shouldRasterize")]
		bool ShouldRasterize { get; set; }

		[Export ("blendMode")]
		SKBlendMode BlendMode { get; set; }

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[NullAllowed] // by default this property is null
		[Export ("shader", ArgumentSemantic.Retain)]
		SKShader Shader { get; set; }
	}

	delegate Vector3 SKFieldForceEvaluator (/* vector_float3 */ Vector4 position, /* vector_float3 */ Vector4 velocity, float /* float, not CGFloat */ mass, float /* float, not CGFloat */ charge, double time);
		
	[iOS (8,0), Mac(10,10, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	interface SKFieldNode {
		[Export ("region", ArgumentSemantic.Retain)]
		SKRegion Region { get; set; }

		[Export ("strength")]
		float Strength { get; set; } /* float, not CGFloat */

		[Export ("falloff")]
		float Falloff { get; set; } /* float, not CGFloat */

		[Export ("minimumRadius")]

		float MinimumRadius { get; set; } /* float, not CGFloat */

		[Export ("enabled")]
		bool Enabled { [Bind ("isEnabled")] get; set; }

		[Export ("exclusive")]
		bool Exclusive { [Bind ("isExclusive")] get; set; }

		[Export ("categoryBitMask")]
		uint CategoryBitMask { get; set; } /* uint32_t */

		[Export ("direction")]
		/* This was typed as Vector4 since sizeof the native type (vector_float3) = 16 */
		Vector4 Direction {
			[MarshalDirective (NativePrefix = "xamarin_vector_float3__", Library = "__Internal")] get;
			[MarshalDirective (NativePrefix = "xamarin_vector_float3__", Library = "__Internal")] set;
		}

		[Export ("smoothness")]
		float Smoothness { get; set; } /* float, not CGFloat */

		[Export ("animationSpeed")]
		float AnimationSpeed { get; set; } /* float, not CGFloat */

		[Export ("texture", ArgumentSemantic.Retain)]
		SKTexture Texture { get; set; }

		[Static, Export ("dragField")]
		SKFieldNode CreateDragField ();

		[Static, Export ("vortexField")]
		SKFieldNode CreateVortexField ();

		[Static, Export ("radialGravityField")]
		SKFieldNode CreateRadialGravityField ();

		[Static, Export ("linearGravityFieldWithVector:")]
		SKFieldNode CreateLinearGravityField (/* vector_float3 */ Vector4 direction);

		[Static, Export ("velocityFieldWithVector:")]
		SKFieldNode CreateVelocityField (/* vector_float3 */ Vector4 direction);

		[Static, Export ("velocityFieldWithTexture:")]
		SKFieldNode CreateVelocityField (SKTexture velocityTexture);

		[Static, Export ("noiseFieldWithSmoothness:animationSpeed:")]
		SKFieldNode CreateNoiseField (nfloat smoothness, nfloat speed);

		[Static, Export ("turbulenceFieldWithSmoothness:animationSpeed:")]
		SKFieldNode CreateTurbulenceField (nfloat smoothness, nfloat speed);

		[Static, Export ("springField")]
		SKFieldNode CreateSpringField ();

		[Static, Export ("electricField")]
		SKFieldNode CreateElectricField ();

		[Static, Export ("magneticField")]
		SKFieldNode CreateMagneticField ();

		[Static, Export ("customFieldWithEvaluationBlock:")]
		SKFieldNode CreateCustomField (SKFieldForceEvaluator evaluator);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKEffectNode))]
	partial interface SKScene {
		[Export ("initWithSize:")]
		IntPtr Constructor (CGSize size);

		[Static, Export ("sceneWithSize:")]
		SKScene FromSize (CGSize size);

		[Export ("size")]
		CGSize Size { get; set; }

		[Export ("scaleMode")]
		SKSceneScaleMode ScaleMode { get; set; }

		[Export ("backgroundColor", ArgumentSemantic.Retain)]
		UIColor BackgroundColor { get; set; }

		[Export ("anchorPoint")]
		CGPoint AnchorPoint { get; set; }

		[Export ("physicsWorld")]
		SKPhysicsWorld PhysicsWorld { get; }

		[Export ("convertPointFromView:")]
		CGPoint ConvertPointFromView (CGPoint point);

		[Export ("convertPointToView:")]
		CGPoint ConvertPointToView (CGPoint point);

		[Export ("view", ArgumentSemantic.Weak)]
		SKView View { get; }

		[Export ("update:")]
		void Update (double currentTime);

		[Export ("didEvaluateActions")]
		void DidEvaluateActions ();

		[Export ("didSimulatePhysics")]
		void DidSimulatePhysics ();

		[Export ("didMoveToView:")]
		void DidMoveToView (SKView view);

		[Export ("willMoveFromView:")]
		void WillMoveFromView (SKView view);

		[Export ("didChangeSize:")]
		void DidChangeSize (CGSize oldSize);

		[iOS (8,0), Mac(10,10)]
		[Export ("didApplyConstraints")]
		void DidApplyConstraints ();

		[iOS (8,0), Mac(10,10)]
		[Export ("didFinishUpdate")]
		void DidFinishUpdate ();

		[iOS (8,0), Mac(10,10)]
		[Export ("delegate", ArgumentSemantic.Assign), NullAllowed]
		NSObject WeakDelegate { get; set;}

		[iOS (8,0), Mac(10,10)]
		[Wrap ("WeakDelegate")]
		[Protocolize]
		SKSceneDelegate Delegate { get; set; }
		
		[iOS (9,0),Mac(10,11)]
		[Export ("audioEngine", ArgumentSemantic.Retain)]
		AVAudioEngine AudioEngine { get; }

		[iOS (9,0), Mac(10,11)]
		[NullAllowed, Export ("camera", ArgumentSemantic.Weak)]
		SKCameraNode Camera { get; set; }

		[iOS (9,0), Mac(10,11)]
		[NullAllowed, Export ("listener", ArgumentSemantic.Weak)]
		SKNode Listener { get; set; }
	}

	[iOS (8,0), Mac(10,10, onlyOn64 : true)]
	[Protocol, Model]
	[BaseType (typeof (NSObject))]
	interface SKSceneDelegate {
		[Export ("update:forScene:")]
		void Update (double currentTime, SKScene scene);

		[Export ("didEvaluateActionsForScene:")]
		void DidEvaluateActions (SKScene scene);

		[Export ("didSimulatePhysicsForScene:")]
		void DidSimulatePhysics (SKScene scene);

		[Export ("didApplyConstraintsForScene:")]
		void DidApplyConstraints (SKScene scene);

		[Export ("didFinishUpdateForScene:")]
		void DidFinishUpdate (SKScene scene);
	}

	[Mac (10,10, onlyOn64 : true)]
	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	interface SKShader : NSCopying, NSCoding {
		[Export ("initWithSource:")]
		IntPtr Constructor (string shaderSourceCode);

		[Export ("initWithSource:uniforms:")]
		IntPtr Constructor (string sharedSourceCode, SKUniform [] uniforms);

		[NullAllowed] // by default this property is null
		[Export ("source")]
		string Source { get; set; }

		// @property (copy) NSArray * uniforms;
		[Export ("uniforms", ArgumentSemantic.Copy)]
		SKUniform [] Uniforms { get; set; }

		// @required + (instancetype)shader;
		[Static, Export ("shader")]
		SKShader Create ();

		[Static, Export ("shaderWithSource:")]
		SKShader FromShaderSourceCode (string source);

		[Static, Export ("shaderWithSource:uniforms:")]
		SKShader FromShaderSourceCode (string source, SKUniform [] uniforms);

		[Static, Export ("shaderWithFileNamed:")]
		SKShader FromFile (string name);

		[Export ("addUniform:")]
		void AddUniform (SKUniform uniform);

		[Export ("uniformNamed:")]
		SKUniform GetUniform (string uniformName);

		[Export ("removeUniformNamed:")]
		void RemoveUniform (string uniforName);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKNode))]
	partial interface SKSpriteNode {

		[Static, Export ("spriteNodeWithTexture:size:")]
		SKSpriteNode FromTexture ([NullAllowed] SKTexture texture, CGSize size);

		[Static, Export ("spriteNodeWithTexture:")]
		SKSpriteNode FromTexture ([NullAllowed] SKTexture texture);

		[Static, Export ("spriteNodeWithImageNamed:")]
		SKSpriteNode FromImageNamed (string name);

		[Static, Export ("spriteNodeWithColor:size:")]
		SKSpriteNode FromColor ([NullAllowed] UIColor color, CGSize size);

		[DesignatedInitializer]
		[Export ("initWithTexture:color:size:")]
		IntPtr Constructor ([NullAllowed] SKTexture texture, [NullAllowed] UIColor color, CGSize size);

		[Export ("initWithTexture:")]
		IntPtr Constructor ([NullAllowed] SKTexture texture);

		// can't be null -> crash
		[Export ("initWithImageNamed:")]
		IntPtr Constructor (string name);

		[Export ("initWithColor:size:")]
		IntPtr Constructor ([NullAllowed] UIColor color, CGSize size);

		[Export ("texture", ArgumentSemantic.Retain)]
		[NullAllowed]
		SKTexture Texture { get; set; }

		[Export ("centerRect")]
		CGRect CenterRect { get; set; }

		[Export ("colorBlendFactor")]
		nfloat ColorBlendFactor { get; set; }

		[Export ("color", ArgumentSemantic.Retain)]
		[NullAllowed]
		UIColor Color { get; set; }

		[Export ("blendMode")]
		SKBlendMode BlendMode { get; set; }

		[Export ("anchorPoint")]
		CGPoint AnchorPoint { get; set; }

		[Export ("size")]
		CGSize Size { get; set; }

		//
		// iOS 8
		//

		
		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("spriteNodeWithTexture:normalMap:")]
		SKSpriteNode Create ([NullAllowed] SKTexture texture, [NullAllowed] SKTexture normalMap);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("spriteNodeWithImageNamed:normalMapped:")]
		SKSpriteNode Create (string imageName, bool generateNormalMap);
		
		[iOS (8,0), Mac (10,10)]
		[NullAllowed] // by default this property is null
		[Export ("normalTexture", ArgumentSemantic.Retain)]
		SKTexture NormalTexture { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("lightingBitMask")]
		uint LightingBitMask { get; set; } /* uint32_t */

		[iOS (8,0), Mac (10,10)]
		[Export ("shadowCastBitMask")]
		uint ShadowCastBitMask { get; set; } /* uint32_t */

		[iOS (8,0), Mac (10,10)]
		[Export ("shadowedBitMask")]
		uint ShadowedBitMask { get; set; } /* uint32_t */

		[iOS (8,0), Mac (10,10)]
		[Export ("shader", ArgumentSemantic.Retain), NullAllowed]
		SKShader Shader { get; set; }		
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	partial interface SKKeyframeSequence : NSCoding, NSCopying {

		[DesignatedInitializer]
		[Export ("initWithKeyframeValues:times:")]
		[Internal]
		IntPtr Constructor ([NullAllowed] NSObject [] values, [NullAllowed] NSArray times);

		[Export ("initWithCapacity:")]
		IntPtr Constructor (nuint numItems);

		[Export ("count")]
		nuint Count { get; }

		[Export ("addKeyframeValue:time:")]
		void AddKeyframeValue (NSObject value, nfloat time);

		[Export ("removeLastKeyframe")]
		void RemoveLastKeyframe ();

		[Export ("removeKeyframeAtIndex:")]
		void RemoveKeyframe (nuint index);

		[Export ("setKeyframeValue:forIndex:")]
		void SetKeyframeValue (NSObject value, nuint index);

		[Export ("setKeyframeTime:forIndex:")]
		void SetKeyframeTime (nfloat time, nuint index);

		[Export ("setKeyframeValue:time:forIndex:")]
		void SetKeyframeValue (NSObject value, nfloat time, nuint index);

		[Export ("getKeyframeValueForIndex:")]
		NSObject GetKeyframeValue (nuint index);

		[Export ("getKeyframeTimeForIndex:")]
		nfloat GetKeyframeTime (nuint index);

		[Export ("sampleAtTime:")]
		NSObject SampleAtTime (nfloat time);

		[Export ("interpolationMode")]
		SKInterpolationMode InterpolationMode { get; set; }

		[Export ("repeatMode")]
		SKRepeatMode RepeatMode { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKNode))]
	partial interface SKEmitterNode {

		[Export ("advanceSimulationTime:")]
		void AdvanceSimulationTime (double sec);

		[Export ("resetSimulation")]
		void ResetSimulation ();

		[NullAllowed] // by default this property is null
		[Export ("particleTexture", ArgumentSemantic.Retain)]
		SKTexture ParticleTexture { get; set; }

		[Export ("particleZPosition")]
		nfloat ParticleZPosition { get; set; }

		[Export ("particleZPositionRange")]
		nfloat ParticleZPositionRange { get; set; }

		[Export ("particleBlendMode")]
		SKBlendMode ParticleBlendMode { get; set; }

		[Export ("particleColor", ArgumentSemantic.Retain)]
		UIColor ParticleColor { get; set; }

		[Export ("particleColorRedRange")]
		nfloat ParticleColorRedRange { get; set; }

		[Export ("particleColorGreenRange")]
		nfloat ParticleColorGreenRange { get; set; }

		[Export ("particleColorBlueRange")]
		nfloat ParticleColorBlueRange { get; set; }

		[Export ("particleColorAlphaRange")]
		nfloat ParticleColorAlphaRange { get; set; }

		[Export ("particleColorRedSpeed")]
		nfloat ParticleColorRedSpeed { get; set; }

		[Export ("particleColorGreenSpeed")]
		nfloat ParticleColorGreenSpeed { get; set; }

		[Export ("particleColorBlueSpeed")]
		nfloat ParticleColorBlueSpeed { get; set; }

		[Export ("particleColorAlphaSpeed")]
		nfloat ParticleColorAlphaSpeed { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("particleColorSequence", ArgumentSemantic.Retain)]
		SKKeyframeSequence ParticleColorSequence { get; set; }

		[Export ("particleColorBlendFactor")]
		nfloat ParticleColorBlendFactor { get; set; }

		[Export ("particleColorBlendFactorRange")]
		nfloat ParticleColorBlendFactorRange { get; set; }

		[Export ("particleColorBlendFactorSpeed")]
		nfloat ParticleColorBlendFactorSpeed { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("particleColorBlendFactorSequence", ArgumentSemantic.Retain)]
		SKKeyframeSequence ParticleColorBlendFactorSequence { get; set; }

		[Export ("particlePosition")]
		CGPoint ParticlePosition { get; set; }

		[Export ("particlePositionRange")]
		CGVector ParticlePositionRange { get; set; }

		[Export ("particleSpeed")]
		nfloat ParticleSpeed { get; set; }

		[Export ("particleSpeedRange")]
		nfloat ParticleSpeedRange { get; set; }

		[Export ("emissionAngle")]
		nfloat EmissionAngle { get; set; }

		[Export ("emissionAngleRange")]
		nfloat EmissionAngleRange { get; set; }

		[Export ("xAcceleration")]
		nfloat XAcceleration { get; set; }

		[Export ("yAcceleration")]
		nfloat YAcceleration { get; set; }

		[Export ("particleBirthRate")]
		nfloat ParticleBirthRate { get; set; }

		[Export ("numParticlesToEmit")]
		nuint NumParticlesToEmit { get; set; }

		[Export ("particleLifetime")]
		nfloat ParticleLifetime { get; set; }

		[Export ("particleLifetimeRange")]
		nfloat ParticleLifetimeRange { get; set; }

		[Export ("particleRotation")]
		nfloat ParticleRotation { get; set; }

		[Export ("particleRotationRange")]
		nfloat ParticleRotationRange { get; set; }

		[Export ("particleRotationSpeed")]
		nfloat ParticleRotationSpeed { get; set; }

		[Export ("particleSize")]
		CGSize ParticleSize { get; set; }

		[Export ("particleScale")]
		nfloat ParticleScale { get; set; }

		[Export ("particleScaleRange")]
		nfloat ParticleScaleRange { get; set; }

		[Export ("particleScaleSpeed")]
		nfloat ParticleScaleSpeed { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("particleScaleSequence", ArgumentSemantic.Retain)]
		SKKeyframeSequence ParticleScaleSequence { get; set; }

		[Export ("particleAlpha")]
		nfloat ParticleAlpha { get; set; }

		[Export ("particleAlphaRange")]
		nfloat ParticleAlphaRange { get; set; }

		[Export ("particleAlphaSpeed")]
		nfloat ParticleAlphaSpeed { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("particleAlphaSequence", ArgumentSemantic.Retain)]
		SKKeyframeSequence ParticleAlphaSequence { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("particleAction", ArgumentSemantic.Copy)]
		SKAction ParticleAction { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("targetNode", ArgumentSemantic.Weak)]
		SKNode TargetNode { get; set; }

		//
		// iOS 8
		//
		[iOS (8,0), Mac(10,10)]
		[Export ("fieldBitMask")]
		uint FieldBitMask { get; set; } /* uint32_t */

		[iOS (8,0), Mac(10,10)]
		[Export ("particleZPositionSpeed")]
		nfloat ParticleZPositionSpeed { get; set; }

		[iOS (8,0), Mac(10,10)]
		[NullAllowed] // by default this property is null
		[Export ("shader", ArgumentSemantic.Retain)]
		SKShader Shader { get; set; }

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Export ("particleRenderOrder", ArgumentSemantic.Assign)]
		SKParticleRenderOrder ParticleRenderOrder { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKNode))]
	partial interface SKShapeNode {

		[NullAllowed]
		[Export ("path")]
		CGPath Path { get; set; }

		[Export ("strokeColor", ArgumentSemantic.Retain)]
		UIColor StrokeColor { get; set; }

		[Export ("fillColor", ArgumentSemantic.Retain)]
		UIColor FillColor { get; set; }

		[Export ("blendMode")]
		SKBlendMode BlendMode { get; set; }

		[Export ("antialiased")]
		bool Antialiased { [Bind ("isAntialiased")] get; set; }

		[Export ("lineWidth")]
		nfloat LineWidth { get; set; }

		[Export ("glowWidth")]
		nfloat GlowWidth { get; set; }

		//
		// iOS 8
		//
		[iOS (8,0), Mac (10,10)]
		[NullAllowed] // by default this property is null
		[Export ("fillTexture", ArgumentSemantic.Retain)]
		SKTexture FillTexture { get; set; }

		[iOS (8,0), Mac (10,10)]
		[NullAllowed] // by default this property is null
		[Export ("fillShader", ArgumentSemantic.Retain)]
		SKShader FillShader { get; set; }

		[iOS (8,0), Mac (10,10)]
		[NullAllowed] // by default this property is null
		[Export ("strokeTexture", ArgumentSemantic.Retain)]
		SKTexture StrokeTexture { get; set; }

		[iOS (8,0), Mac (10,10)]
		[NullAllowed] // by default this property is null
		[Export ("strokeShader", ArgumentSemantic.Retain)]
		SKShader StrokeShader { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithPath:")]
		SKShapeNode FromPath (CGPath path);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithPath:centered:")]
		SKShapeNode FromPath (CGPath path, bool centered);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithRect:")]
		SKShapeNode FromRect (CGRect rect);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithRectOfSize:")]
		SKShapeNode FromRect (CGSize size);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithRect:cornerRadius:")]
		SKShapeNode FromRect (CGRect rect, nfloat cornerRadius);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithRectOfSize:cornerRadius:")]
		SKShapeNode FromRect (CGSize size, nfloat cornerRadius);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithCircleOfRadius:")]
		SKShapeNode FromCircle (nfloat radius);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithEllipseInRect:")]
		SKShapeNode FromEllipse (CGRect rect);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithEllipseOfSize:")]
		SKShapeNode FromEllipse (CGSize size);

#if XAMCORE_3_0 // Hide this ugly api fixes https://bugzilla.xamarin.com/show_bug.cgi?id=39706
		[Internal]
#endif
		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithPoints:count:")]
		SKShapeNode FromPoints (ref CGPoint points, nuint numPoints);

#if XAMCORE_3_0 // Hide this ugly api fixes https://bugzilla.xamarin.com/show_bug.cgi?id=39706
		[Internal]
#endif
		[iOS (8,0), Mac (10,10)]
		[Static, Export ("shapeNodeWithSplinePoints:count:")]
		SKShapeNode FromSplinePoints (ref CGPoint points, nuint numPoints);

		[iOS (8,0), Mac (10,10)]
		[Export ("lineCap")]
		CGLineCap LineCap { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("lineJoin")]
		CGLineJoin LineJoin { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("miterLimit")]
		nfloat MiterLimit { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("lineLength")]
		nfloat LineLength { get; }
	}

	[iOS (8,0), Mac(10,10, onlyOn64 : true)]
	[BaseType (typeof (NSObject))]
	interface SKReachConstraints : NSCoding {
		[DesignatedInitializer]
		[Export ("initWithLowerAngleLimit:upperAngleLimit:")]
		IntPtr Constructor (nfloat lowerAngleLimit, nfloat upperAngleLimit);

		[Export ("lowerAngleLimit", ArgumentSemantic.UnsafeUnretained)]
		nfloat LowerAngleLimit { get; set; }

		[Export ("upperAngleLimit", ArgumentSemantic.UnsafeUnretained)]
		nfloat UpperAngleLimit { get; set; }
	}

	[iOS(8,0), Mac(10,10, onlyOn64 : true)]
	[BaseType (typeof (NSObject))]
	interface SKRegion : NSCopying, NSCoding {
		[Export ("initWithRadius:")]
		IntPtr Constructor (float /* float, not CGFloat */ radius);

		[Export ("initWithSize:")]
		IntPtr Constructor (CGSize size);

		[Export ("initWithPath:")]
		IntPtr Constructor (CGPath path);

		[Export ("path")]
		CGPath Path { get; }

		[Static, Export ("infiniteRegion")]
		SKRegion InfiniteRegion  { get; }

		[Export ("inverseRegion")]
		SKRegion InverseRegion ();

		[Export ("regionByUnionWithRegion:")]
		SKRegion CreateUnion (SKRegion region);

		[Export ("regionByDifferenceFromRegion:")]
		SKRegion CreateDifference (SKRegion region);

		[Export ("regionByIntersectionWithRegion:")]
		SKRegion CreateIntersection (SKRegion region);

		[Export ("containsPoint:")]
		bool ContainsPoint (CGPoint point);
	}

	[Since (7,0)]
	[Mac (10,9, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	partial interface SKLabelNode {

		[Static, Export ("labelNodeWithFontNamed:")]
		SKLabelNode FromFont ([NullAllowed] string fontName);

		[Export ("initWithFontNamed:")]
		IntPtr Constructor ([NullAllowed] string fontName);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("labelNodeWithText:")]
		SKLabelNode FromText ([NullAllowed] string text);

		[Export ("verticalAlignmentMode")]
		SKLabelVerticalAlignmentMode VerticalAlignmentMode { get; set; }

		[Export ("horizontalAlignmentMode")]
		SKLabelHorizontalAlignmentMode HorizontalAlignmentMode { get; set; }

		[Export ("fontName", ArgumentSemantic.Copy)]
		string FontName { get; set; }

		[Export ("text", ArgumentSemantic.Copy)]
		[NullAllowed] // nullable in Xcode7 headers and caught by introspection tests
		string Text { get; set; }

		[Export ("fontSize")]
		nfloat FontSize { get; set; }

		[Export ("fontColor", ArgumentSemantic.Retain)]
		UIColor FontColor { get; set; }

		[Export ("colorBlendFactor")]
		nfloat ColorBlendFactor { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("color", ArgumentSemantic.Retain)]
		UIColor Color { get; set; }

		[Export ("blendMode")]
		SKBlendMode BlendMode { get; set; }
	}

	[iOS (8,0), Mac(10,10, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	interface SKLightNode {
		[Export ("enabled")]
		bool Enabled { [Bind ("isEnabled")] get; set; }

		[Export ("lightColor")]
		UIColor LightColor { get; set; }

		[Export ("ambientColor")]
		UIColor AmbientColor { get; set; }

		[Export ("shadowColor")]
		UIColor ShadowColor { get; set; }

		[Export ("falloff")]
		nfloat Falloff { get; set; }

		[Export ("categoryBitMask")]
		uint CategoryBitMask { get; set; } /* uint32_t */
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKNode))]
	partial interface SKVideoNode {

		[Static, Export ("videoNodeWithAVPlayer:")]
		SKVideoNode FromPlayer (AVPlayer player);

		[Static, Export ("videoNodeWithVideoFileNamed:"), Internal]
		SKVideoNode VideoNodeWithVideoFileNamed (string videoFile);

		[Static, Export ("videoNodeWithFileNamed:"), Internal]
		SKVideoNode VideoNodeWithFileNamed (string videoFile);

		[Static, Export ("videoNodeWithVideoURL:"), Internal]
		SKVideoNode VideoNodeWithVideoURL (NSUrl videoURL);

		[Static, Export ("videoNodeWithURL:"), Internal]
		SKVideoNode VideoNodeWithURL (NSUrl videoURL);

		[DesignatedInitializer]
		[Export ("initWithAVPlayer:")]
		IntPtr Constructor (AVPlayer player);

		[Export ("initWithVideoFileNamed:"), Internal]
		IntPtr InitWithVideoFileNamed (string videoFile);

		[Export ("initWithFileNamed:"), Internal]
		IntPtr InitWithFileNamed (string videoFile);

		[Export ("initWithVideoURL:"), Internal]
		IntPtr InitWithVideoURL (NSUrl url);

		[Export ("initWithURL:"), Internal]
		IntPtr InitWithURL (NSUrl url);

		[Export ("play")]
		void Play ();

		[Export ("pause")]
		void Pause ();

		[Export ("size")]
		CGSize Size { get; set; }

		[Export ("anchorPoint")]
		CGPoint AnchorPoint { get; set; }
	}

	[iOS (8,0)]
	[Mac (10,10, onlyOn64 : true)]
	[BaseType (typeof (NSObject))]
	interface SKConstraint : NSCoding, NSCopying {
		[Export ("enabled")]
		bool Enabled { get; set; }

		[NullAllowed] // by default this property is null
		[Export ("referenceNode", ArgumentSemantic.Retain)]
		SKNode ReferenceNode { get; set; }

		[Static, Export ("positionX:")]
		SKConstraint CreateXRestriction (SKRange range);

		[Static, Export ("positionY:")]
		SKConstraint CreateYRestriction (SKRange range);

		[Static, Export ("positionX:Y:")]
		SKConstraint CreateRestriction (SKRange xRange, SKRange yRange);

		[Static, Export ("distance:toNode:")]
		SKConstraint CreateDistance (SKRange range, SKNode node);

		[Static, Export ("distance:toPoint:")]
		SKConstraint CreateDistance (SKRange range, CGPoint point);

		[Static, Export ("distance:toPoint:inNode:")]
		SKConstraint CreateDistance (SKRange range, CGPoint point, SKNode node);

		[Static, Export ("zRotation:")]
		SKConstraint CreateZRotation (SKRange zRange);

		[Static, Export ("orientToNode:offset:")]
		SKConstraint CreateOrientToNode (SKNode node, SKRange radians);

		[Static, Export ("orientToPoint:offset:")]
		SKConstraint CreateOrientToPoint (CGPoint point, SKRange radians);

		[Static, Export ("orientToPoint:inNode:offset:")]
		SKConstraint CreateOrientToPoint (CGPoint point, SKNode node, SKRange radians);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKNode))]
	partial interface SKCropNode {

		[NullAllowed] // by default this property is null
		[Export ("maskNode", ArgumentSemantic.Retain)]
		SKNode MaskNode { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (UIView))]
#if XAMCORE_3_0
	[DisableDefaultCtor]
#endif
	partial interface SKView {

		[Export ("initWithFrame:")]
		IntPtr Constructor (CGRect frame);

		[Export ("paused")]
		bool Paused { [Bind ("isPaused")] get; set; }

		[Export ("showsFPS")]
		bool ShowsFPS { get; set; }

		[Export ("showsDrawCount")]
		bool ShowsDrawCount { get; set; }

		[Export ("showsNodeCount")]
		bool ShowsNodeCount { get; set; }

		[Since (7,1), Mac(10,10)]
		[Export ("showsPhysics")]
		bool ShowsPhysics { get; set; }

		[Export ("asynchronous")]
		bool Asynchronous { [Bind ("isAsynchronous")] get; set; }

		[Export ("frameInterval")]
		nint FrameInterval { get; set; }

		[Export ("presentScene:")]
		void PresentScene ([NullAllowed] SKScene scene);

		[Export ("presentScene:transition:")]
		void PresentScene (SKScene scene, SKTransition transition);

		[Export ("scene")]
		SKScene Scene { get; }

		[Export ("textureFromNode:")]
		SKTexture TextureFromNode (SKNode node);

		[Export ("convertPoint:toScene:")]
		CGPoint ConvertPointToScene (CGPoint point, SKScene scene);

		[Export ("convertPoint:fromScene:")]
		CGPoint ConvertPointFromScene (CGPoint point, SKScene scene);

		[Export ("ignoresSiblingOrder")]
		bool IgnoresSiblingOrder { get; set; }

		//
		// iOS 8
		//
		[iOS (8,0), Mac(10,10)]
		[Export ("allowsTransparency")]
		bool AllowsTransparency { get; set; }

		[iOS (8,0), Mac(10,10)]
		[Export ("shouldCullNonVisibleNodes")]
		bool ShouldCullNonVisibleNodes { get; set; }

		[iOS (8,0), Mac(10,10)]
		[Export ("showsFields")]
		bool ShowsFields { get; set; }
		
		[iOS (8,0), Mac(10,10)]
		[Export ("showsQuadCount")]
		bool ShowsQuadCount { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("textureFromNode:crop:")]
		SKTexture TextureFromNode (SKNode node, CGRect crop);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor]
	partial interface SKTransition : NSCopying {

		[Static, Export ("crossFadeWithDuration:")]
		SKTransition CrossFadeWithDuration (double sec);

		[Static, Export ("fadeWithDuration:")]
		SKTransition FadeWithDuration (double sec);

		[Static, Export ("fadeWithColor:duration:")]
		SKTransition FadeWithColor (UIColor color, double sec);

		[Static, Export ("flipHorizontalWithDuration:")]
		SKTransition FlipHorizontalWithDuration (double sec);

		[Static, Export ("flipVerticalWithDuration:")]
		SKTransition FlipVerticalWithDuration (double sec);

		[Static, Export ("revealWithDirection:duration:")]
		SKTransition RevealWithDirection (SKTransitionDirection direction, double sec);

		[Static, Export ("moveInWithDirection:duration:")]
		SKTransition MoveInWithDirection (SKTransitionDirection direction, double sec);

		[Static, Export ("pushWithDirection:duration:")]
		SKTransition PushWithDirection (SKTransitionDirection direction, double sec);

		[Static, Export ("doorsOpenHorizontalWithDuration:")]
		SKTransition DoorsOpenHorizontalWithDuration (double sec);

		[Static, Export ("doorsOpenVerticalWithDuration:")]
		SKTransition DoorsOpenVerticalWithDuration (double sec);

		[Static, Export ("doorsCloseHorizontalWithDuration:")]
		SKTransition DoorsCloseHorizontalWithDuration (double sec);

		[Static, Export ("doorsCloseVerticalWithDuration:")]
		SKTransition DoorsCloseVerticalWithDuration (double sec);

		[Static, Export ("doorwayWithDuration:")]
		SKTransition DoorwayWithDuration (double sec);

		[Static, Export ("transitionWithCIFilter:duration:")]
		SKTransition TransitionWithCIFilter (CIFilter filter, double sec);

		[Export ("pausesIncomingScene")]
		bool PausesIncomingScene { get; set; }

		[Export ("pausesOutgoingScene")]
		bool PausesOutgoingScene { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor]
	partial interface SKTexture : NSCoding, NSCopying {

		[Static, Export ("textureWithImageNamed:")]
		SKTexture FromImageNamed (string name);

		[Static, Export ("textureWithRect:inTexture:")]
		SKTexture FromRectangle (CGRect rect, SKTexture texture);

		[Static, Export ("textureWithCGImage:")]
		SKTexture FromImage (CGImage image);

		[Static, Export ("textureWithImage:")]
		SKTexture FromImage (UIImage image);

		[Static, Export ("textureWithData:size:")]
		SKTexture FromData (NSData pixelData, CGSize size);

		[Static, Export ("textureWithData:size:rowLength:alignment:")]
		SKTexture FromData (NSData pixelData, CGSize size, uint /* unsigned int*/ rowLength, uint /* unsigned int */ alignment);

		[Export ("textureByApplyingCIFilter:")]
		SKTexture TextureByApplyingCIFilter (CIFilter filter);

		[Export ("textureRect")]
		CGRect TextureRect { get; }

		[Export ("size")]
		CGSize Size { get; }

		[Export ("filteringMode")]
		SKTextureFilteringMode FilteringMode { get; set; }

		[Export ("usesMipmaps")]
		bool UsesMipmaps { get; set; }

		[Static]
		[Export ("preloadTextures:withCompletionHandler:")]
		[Async]
		// note: unlike SKTextureAtlas completion can't be null (or it crash)
		void PreloadTextures (SKTexture [] textures, NSAction completion);

		[Export ("preloadWithCompletionHandler:")]
		[Async]
		// note: unlike SKTextureAtlas completion can't be null (or it crash)
		void Preload (NSAction completion);

		[iOS (8,0), Mac (10,10)]
		[Export ("textureByGeneratingNormalMap")]
		SKTexture CreateTextureByGeneratingNormalMap ();

		[iOS (8,0), Mac (10,10)]
		[Export ("textureByGeneratingNormalMapWithSmoothness:contrast:")]
		SKTexture CreateTextureByGeneratingNormalMap (nfloat smoothness, nfloat contrast);

		[iOS (8,0), Mac(10,10)]
		[Static, Export ("textureVectorNoiseWithSmoothness:size:")]
		SKTexture FromTextureVectorNoise (nfloat smoothness, CGSize size);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("textureNoiseWithSmoothness:size:grayscale:")]
		SKTexture FromTextureNoise (nfloat smoothness, CGSize size, bool grayscale);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("textureWithData:size:flipped:")]
		SKTexture FromData (NSData pixelData, CGSize size, bool flipped);

		[iOS (9,0), Mac (10,11)]
		[Export ("CGImage")]
		CGImage CGImage { get; }
	}

	delegate void SKTextureModify (IntPtr pixelData, nuint lengthInBytes);
		
	[iOS (8,0), Mac(10,10, onlyOn64 : true)]
	[BaseType (typeof (SKTexture))]
	[DisableDefaultCtor] // cannot be created (like SKTexture) by calling `init`
	interface SKMutableTexture {
		[Export ("initWithSize:")]
		IntPtr Constructor (CGSize size);

		[Export ("initWithSize:pixelFormat:")]
		IntPtr Constructor (CGSize size, CVPixelFormatType pixelFormat);

		[Static, Export ("mutableTextureWithSize:")]
		SKMutableTexture Create (CGSize size);

		[Export ("modifyPixelDataWithBlock:")]
		void ModifyPixelData (SKTextureModify modifyMethod);
	}

	delegate void SKTextureAtlasLoadCallback (NSError error, SKTextureAtlas foundAtlases);
		
	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	partial interface SKTextureAtlas : NSCoding {

		[Static, Export ("atlasNamed:")]
		SKTextureAtlas FromName (string name);

		[Export ("textureNamed:")]
		SKTexture TextureNamed (string name);

		[Export ("textureNames")]
		string [] TextureNames { get; }

		[Static]
		[Export ("preloadTextureAtlases:withCompletionHandler:")]
		[Async]
		// Unfortunate name, should have been PreloadTextureAtlases
		void PreloadTextures (SKTextureAtlas [] textures, [NullAllowed] NSAction completion);

		[iOS (9,0), Mac(10,11)]
		[Static]
		[Export ("preloadTextureAtlasesNamed:withCompletionHandler:")]
		[Async(ResultTypeName="SKTextureAtlasLoadResult")]
		void PreloadTextureAtlases (string[] atlasNames, SKTextureAtlasLoadCallback completionHandler);

		[Export ("preloadWithCompletionHandler:")]
		[Async]
		void Preload ([NullAllowed] NSAction completion);

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("atlasWithDictionary:")]
		SKTextureAtlas FromDictionary (NSDictionary properties);
		
	}

	[Mac (10,10, onlyOn64 : true)]
	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	interface SKUniform : NSCopying, NSCoding {
		[Export ("initWithName:")]
		IntPtr Constructor (string name);

		[Export ("initWithName:texture:")]
		IntPtr Constructor (string name, SKTexture texture);

		[Export ("initWithName:float:")]
		IntPtr Constructor (string name, float /* float, not CGFloat */ value);

		[Export ("initWithName:floatVector2:")]
		IntPtr Constructor (string name, Vector2 value);

		[Export ("initWithName:floatVector3:")]
		IntPtr Constructor (string name, Vector3 value);

		[Export ("initWithName:floatVector4:")]
		IntPtr Constructor (string name, Vector4 value);

		[Export ("initWithName:floatMatrix2:")]
		IntPtr Constructor (string name, Matrix2 value);

		[Export ("initWithName:floatMatrix3:")]
		IntPtr Constructor (string name, Matrix3 value);

		[Export ("initWithName:floatMatrix4:")]
		IntPtr Constructor (string name, Matrix4 value);

		[Export ("name")]
		string Name { get; }

		[Export ("uniformType")]
		SKUniformType UniformType { get; }

		[Export ("textureValue")]
		SKTexture TextureValue { get; set; }

		[Export ("floatValue")]
		float FloatValue { get; set; } /* float, not CGFloat */

		[Export ("floatVector2Value")]
		Vector2 FloatVector2Value { get; set; }

		[Export ("floatVector3Value")]
		Vector3 FloatVector3Value { get; set; }

		[Export ("floatVector4Value")]
		Vector4 FloatVector4Value { get; set; }

		[Export ("floatMatrix2Value")]
		Matrix2 FloatMatrix2Value { get; set; }

		[Export ("floatMatrix3Value")]
		Matrix3 FloatMatrix3Value { get; set; }

		[Export ("floatMatrix4Value")]
		Matrix4 FloatMatrix4Value { get; set; }

#if false
	// Do we really need these static factory methods, instead of the nice construcotrs we have in C#?
	// Only reason Apple exposed these is because [[foo alloc] init] is annoying
	//
		[Static, Export ("uniformWithName:")]
		SKUniform Create (string name);

		[Static, Export ("uniformWithName:texture:")]
		SKUniform Create (string name, SKTexture texture);

		[Static, Export ("uniformWithName:float:")]
		SKUniform Create (string name, float /* float, not CGFloat */ value);

		[Static, Export ("uniformWithName:floatVector2:")]
		SKUniform Create (string name, GLKVector2 value);

		[Static, Export ("uniformWithName:floatVector3:")]
		SKUniform Create (string name, GLKVector3 value);

		[Static, Export ("uniformWithName:floatVector4:")]
		SKUniform Create (string name, GLKVector4 value);

		[Static, Export ("uniformWithName:floatMatrix2:")]
		SKUniform Create (string name, GLKMatrix2 value);

		[Static, Export ("uniformWithName:floatMatrix3:")]
		SKUniform Create (string name, GLKMatrix3 value);

		[Static, Export ("uniformWithName:floatMatrix4:")]
		SKUniform Create (string name, GLKMatrix4 value);
#endif
	}

	delegate void SKActionDurationHandler (SKNode node, nfloat elapsedTime);

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor] // <quote>To create an action, call the class method for the action you are interested in. </quote>
	partial interface SKAction : NSCoding, NSCopying {

		[Export ("duration")]
		double Duration { get; set; }

		[Export ("timingMode")]
		SKActionTimingMode TimingMode { get; set; }

		[Export ("speed")]
		nfloat Speed { get; set; }

		[Export ("reversedAction")]
		SKAction ReversedAction { get; }
		
		// These are in a category
		[Static, Export ("moveByX:y:duration:")]
		SKAction MoveBy (nfloat deltaX, nfloat deltaY, double sec);

		[Static, Export ("moveBy:duration:")]
		SKAction MoveBy (CGVector delta, double duration);

		[Static, Export ("moveTo:duration:")]
		SKAction MoveTo (CGPoint location, double sec);

		[Static, Export ("moveToX:duration:")]
		SKAction MoveToX (nfloat x, double sec);

		[Static, Export ("moveToY:duration:")]
		SKAction MoveToY (nfloat y, double sec);

		[Static, Export ("rotateByAngle:duration:")]
		SKAction RotateByAngle (nfloat radians, double sec);

		[Static, Export ("rotateToAngle:duration:")]
		SKAction RotateToAngle (nfloat radians, double sec);

		[Static, Export ("rotateToAngle:duration:shortestUnitArc:")]
		SKAction RotateToAngle (nfloat radians, double sec, bool shortedUnitArc);

		[Static, Export ("resizeByWidth:height:duration:")]
		SKAction ResizeByWidth (nfloat width, nfloat height, double duration);

		[Static, Export ("resizeToWidth:height:duration:")]
		SKAction ResizeTo (nfloat width, nfloat height, double duration);

		[Static, Export ("resizeToWidth:duration:")]
		SKAction ResizeToWidth (nfloat width, double duration);

		[Static, Export ("resizeToHeight:duration:")]
		SKAction ResizeToHeight (nfloat height, double duration);

		[Static, Export ("scaleBy:duration:")]
		SKAction ScaleBy (nfloat scale, double sec);

		[Static, Export ("scaleXBy:y:duration:")]
		SKAction ScaleBy (nfloat xScale, nfloat yScale, double sec);

		[Static, Export ("scaleTo:duration:")]
		SKAction ScaleTo (nfloat scale, double sec);

		[Static, Export ("scaleXTo:y:duration:")]
		SKAction ScaleTo (nfloat xScale, nfloat yScale, double sec);

		[Static, Export ("scaleXTo:duration:")]
		SKAction ScaleXTo (nfloat scale, double sec);

		[Static, Export ("scaleYTo:duration:")]
		SKAction ScaleYTo (nfloat scale, double sec);

		[Static, Export ("sequence:")]
		SKAction Sequence ([Params] SKAction [] actions);

		[Static, Export ("group:")]
		SKAction Group ([Params] SKAction [] actions);

		[Static, Export ("repeatAction:count:")]
		SKAction RepeatAction (SKAction action, nuint count);

		[Static, Export ("repeatActionForever:")]
		SKAction RepeatActionForever (SKAction action);

		[Static, Export ("fadeInWithDuration:")]
		SKAction FadeInWithDuration (double sec);

		[Static, Export ("fadeOutWithDuration:")]
		SKAction FadeOutWithDuration (double sec);

		[Static, Export ("fadeAlphaBy:duration:")]
		SKAction FadeAlphaBy (nfloat factor, double sec);

		[Static, Export ("fadeAlphaTo:duration:")]
		SKAction FadeAlphaTo (nfloat alpha, double sec);

		[iOS (7,1), Mac (10,10)]
		[Static, Export ("setTexture:")]
		SKAction SetTexture (SKTexture texture);

		[iOS (7,1), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("setTexture:resize:")]
		SKAction SetTexture (SKTexture texture, bool resize);

		[Static, Export ("animateWithTextures:timePerFrame:")]
		SKAction AnimateWithTextures (SKTexture [] textures, double sec);

		[Static, Export ("animateWithTextures:timePerFrame:resize:restore:")]
		SKAction AnimateWithTextures (SKTexture [] textures, double sec, bool resize, bool restore);

		[Static, Export ("playSoundFileNamed:waitForCompletion:")]
		SKAction PlaySoundFileNamed (string soundFile, bool wait);

		[Static, Export ("colorizeWithColor:colorBlendFactor:duration:")]
		SKAction ColorizeWithColor (UIColor color, nfloat colorBlendFactor, double sec);

		[Static, Export ("colorizeWithColorBlendFactor:duration:")]
		SKAction ColorizeWithColorBlendFactor (nfloat colorBlendFactor, double sec);

		[Static, Export ("followPath:duration:")]
		SKAction FollowPath (CGPath path, double sec);

		[Static, Export ("followPath:asOffset:orientToPath:duration:")]
		SKAction FollowPath (CGPath path, bool offset, bool orient, double sec);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("followPath:speed:")]
		SKAction FollowPath (CGPath path, nfloat speed);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("followPath:asOffset:orientToPath:speed:")]
		SKAction FollowPath (CGPath path, bool offset, bool orient, nfloat speed);

		[Static, Export ("speedBy:duration:")]
		SKAction SpeedBy (nfloat speed, double sec);

		[Static, Export ("speedTo:duration:")]
		SKAction SpeedTo (nfloat speed, double sec);

		[Static, Export ("waitForDuration:")]
		SKAction WaitForDuration (double sec);

		[Static, Export ("waitForDuration:withRange:")]
		SKAction WaitForDuration (double sec, double durationRange);

		[Static, Export ("removeFromParent")]
		SKAction RemoveFromParent ();

		[Static, Export ("performSelector:onTarget:")]
		SKAction PerformSelector (Selector selector, NSObject target);

		[Static, Export ("runBlock:")]
		SKAction Run (Action block);

		[Static, Export ("runBlock:queue:")]
		SKAction Run (Action block, DispatchQueue queue);

		[Static, Export ("runAction:onChildWithName:")]
		SKAction RunAction (SKAction action, string name);

		[Static, Export ("customActionWithDuration:actionBlock:")]
		SKAction CustomActionWithDuration (double seconds, SKActionDurationHandler actionHandler);

		//
		// iOS 8 cluster (a few more are above, as part of their family
		//
		[iOS (8,0), Mac (10,10)]
		[Static, Export ("hide")]
		SKAction Hide ();

		[iOS (8,0), Mac (10,10)]
		[Static, Export ("unhide")]
		SKAction Unhide ();		

		[iOS (8,0), Mac(10,10)]
		[Static, Export ("reachTo:rootNode:duration:")]
		SKAction ReachTo (CGPoint position, SKNode rootNode, double secs);

		[iOS (8,0), Mac(10,10)]
		[Static, Export ("reachTo:rootNode:velocity:")]
		SKAction ReachTo (CGPoint position, SKNode rootNode, nfloat velocity);

		[iOS (8,0), Mac(10,10)]
		[Static, Export ("reachToNode:rootNode:duration:")]
		SKAction ReachToNode (SKNode node, SKNode rootNode, double sec);

		[iOS (8,0), Mac(10,10)]
		[Static, Export ("reachToNode:rootNode:velocity:")]
		SKAction ReachToNode (SKNode node, SKNode rootNode, nfloat velocity);
		
		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("strengthTo:duration:")]
		SKAction StrengthTo (float /* float, not CGFloat */ strength, double sec);

		[iOS (8,0), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.
		[Static, Export ("strengthBy:duration:")]
		SKAction StrengthBy (float /* float, not CGFloat */ strength, double sec);

		[iOS (8,0), Mac (10,10)]
		[NullAllowed, Export ("timingFunction", ArgumentSemantic.Assign)]
		SKActionTimingFunction TimingFunction { get; set; }

		[iOS (8,0), Mac(10,10)]
		[Static, Export ("falloffBy:duration:")]
		SKAction FalloffBy (float /* float, not CGFloat */ to, double duration);

		[iOS (8,0), Mac(10,10)]
		[Static]
		[Export ("falloffTo:duration:")]
		SKAction FalloffTo (float falloff, double sec);

		// iOS 9 cluster
		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("actionNamed:")]
		[return: NullAllowed]
		SKAction Create (string name);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("actionNamed:duration:")]
		[return: NullAllowed]
		SKAction Create (string name, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("actionNamed:fromURL:")]
		[return: NullAllowed]
		SKAction Create (string name, NSUrl url);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("actionNamed:fromURL:duration:")]
		[return: NullAllowed]
		SKAction Create (string name, NSUrl url, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("setNormalTexture:")]
		SKAction SetNormalTexture (SKTexture texture);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("setNormalTexture:resize:")]
		SKAction SetNormalTexture (SKTexture texture, bool resize);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("animateWithNormalTextures:timePerFrame:")]
		SKAction AnimateWithNormalTextures (SKTexture[] textures, double secondsPerFrame);
		
		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("animateWithNormalTextures:timePerFrame:resize:restore:")]
		SKAction AnimateWithNormalTextures (SKTexture[] textures, double secondsPerFrame, bool resize, bool restore);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("play")]
		SKAction CreatePlay ();

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("pause")]
		SKAction CreatePause ();

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("stop")]
		SKAction CreateStop ();

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changePlaybackRateTo:duration:")]
		SKAction CreateChangePlaybackRate (float playbackRate, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changePlaybackRateBy:duration:")]
		SKAction CreateChangePlaybackRateBy (float playbackRate, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeVolumeTo:duration:")]
		SKAction CreateChangeVolume (float newVolume, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeVolumeBy:duration:")]
		SKAction CreateChangeVolumeBy (float by, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeChargeTo:duration:")]
		SKAction CreateChangeChargeTo (float newCharge, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeChargeBy:duration:")]
		SKAction CreateChangeChargeBy (float by, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeMassTo:duration:")]
		SKAction CreateChangeMassTo (float newMass, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeMassBy:duration:")]
		SKAction CreateChangeMassBy (float by, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("applyForce:duration:")]
		SKAction CreateApplyForce (CGVector force, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("applyForce:atPoint:duration:")]
		SKAction CreateApplyForce (CGVector force, CGPoint point, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("applyTorque:duration:")]
		SKAction CreateApplyTorque (nfloat torque, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("applyImpulse:duration:")]
		SKAction CreateApplyImpulse (CGVector impulse, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("applyImpulse:atPoint:duration:")]
		SKAction CreateApplyImpulse (CGVector impulse, CGPoint point, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("applyAngularImpulse:duration:")]
		SKAction CreateApplyAngularImpulse (nfloat impulse, double duration);

		// SKAction_SKAudioNode inlined

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("stereoPanTo:duration:")]
		SKAction CreateStereoPanTo (float target, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("stereoPanBy:duration:")]
		SKAction CreateStereoPanBy (float by, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeReverbTo:duration:")]
		SKAction CreateChangeReverbTo (float target, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeReverbBy:duration:")]
		SKAction CreateChangeReverbBy (float by, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeObstructionTo:duration:")]
		SKAction CreateChangeObstructionTo (float target, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeObstructionBy:duration:")]
		SKAction CreateChangeObstructionBy (float by, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeOcclusionTo:duration:")]
		SKAction CreateChangeOcclusionTo (float target, double duration);

		[iOS (9,0)][Mac (10,11, onlyOn64 : true)]
		[Static]
		[Export ("changeOcclusionBy:duration:")]
		SKAction CreateChangeOcclusionBy (float by, double duration);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[DisableDefaultCtor] // see https://bugzilla.xamarin.com/show_bug.cgi?id=14502
	[BaseType (typeof (NSObject))]
	partial interface SKPhysicsBody : NSCoding, NSCopying {

		[iOS (7,1), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.]
		[Static, Export ("bodyWithBodies:")]
		SKPhysicsBody FromBodies (SKPhysicsBody [] bodies);

		[Static, Export ("bodyWithCircleOfRadius:")]
		SKPhysicsBody CreateCircularBody (nfloat radius);

		[iOS (7,1), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.]
		[Static, Export ("bodyWithCircleOfRadius:center:")]
		SKPhysicsBody CreateCircularBody (nfloat radius, CGPoint center);

		[Static, Export ("bodyWithRectangleOfSize:")]
		SKPhysicsBody CreateRectangularBody (CGSize size);

		[iOS (7,1), Mac (10,10)] // this method is missing the NS_AVAILABLE macro, but it shows up in the 10.10 sdk, but not the 10.9 sdk.]
		[Static, Export ("bodyWithRectangleOfSize:center:")]
		SKPhysicsBody CreateRectangularBody (CGSize size, CGPoint center);

		[Static, Export ("bodyWithPolygonFromPath:")]
		SKPhysicsBody CreateBodyFromPath (CGPath path);

		[Static, Export ("bodyWithEdgeFromPoint:toPoint:")]
		SKPhysicsBody CreateEdge (CGPoint fromPoint, CGPoint toPoint);

		[Static, Export ("bodyWithEdgeChainFromPath:")]
		SKPhysicsBody CreateEdgeChain (CGPath path);

		[Static, Export ("bodyWithEdgeLoopFromPath:")]
		SKPhysicsBody CreateEdgeLoop (CGPath path);

		[Static, Export ("bodyWithEdgeLoopFromRect:")]
		SKPhysicsBody CreateEdgeLoop (CGRect rect);

		[Export ("dynamic")]
		bool Dynamic { [Bind ("isDynamic")] get; set; }

		[Export ("usesPreciseCollisionDetection")]
		bool UsesPreciseCollisionDetection { get; set; }

		[Export ("allowsRotation")]
		bool AllowsRotation { get; set; }

		[Export ("resting")]
		bool Resting { [Bind ("isResting")] get; set; }

		[Export ("friction")]
		nfloat Friction { get; set; }

		[Export ("restitution")]
		nfloat Restitution { get; set; }

		[Export ("linearDamping", ArgumentSemantic.Assign)]
		nfloat LinearDamping { get; set; }

		[Export ("angularDamping", ArgumentSemantic.Assign)]
		nfloat AngularDamping { get; set; }

		[Export ("density")]
		nfloat Density { get; set; }

		[Export ("mass")]
		nfloat Mass { get; set; }

		[Export ("area")]
		nfloat Area { get; }

		[Export ("affectedByGravity", ArgumentSemantic.Assign)]
		bool AffectedByGravity { get; set; }

		[Export ("categoryBitMask", ArgumentSemantic.Assign)]
		uint CategoryBitMask { get; set; } /* uint32_t */

		[Export ("collisionBitMask", ArgumentSemantic.Assign)]
		uint CollisionBitMask { get; set; } /* uint32_t */

		[Export ("contactTestBitMask", ArgumentSemantic.Assign)]
		uint ContactTestBitMask { get; set; } /* uint32_t */

		[Export ("joints")]
		SKPhysicsJoint [] Joints { get; }

		[Export ("node", ArgumentSemantic.Weak)]
		SKNode Node { get; }

		[Export ("velocity")]
		CGVector Velocity { get; set; }

		[Export ("angularVelocity")]
		nfloat AngularVelocity { get; set; }

		[Export ("applyForce:")]
		void ApplyForce (CGVector force);

		[Export ("applyForce:atPoint:")]
		void ApplyForce (CGVector force, CGPoint point);

		[Export ("applyTorque:")]
		void ApplyTorque (nfloat torque);

		[Export ("applyImpulse:")]
		void ApplyImpulse (CGVector impulse);

		[Export ("applyImpulse:atPoint:")]
		void ApplyImpulse (CGVector impulse, CGPoint point);

		[Export ("applyAngularImpulse:")]
		void ApplyAngularImpulse (nfloat impulse);

		[Export ("allContactedBodies")]
		SKPhysicsBody [] AllContactedBodies { get; }

		//
		// iOS 8
		//
		[iOS(8,0), Mac(10,10)]
		[Static, Export ("bodyWithTexture:size:")]
		SKPhysicsBody Create (SKTexture texture, CGSize size);

		[iOS(8,0), Mac(10,10)]
		[Static, Export ("bodyWithTexture:alphaThreshold:size:")]
		SKPhysicsBody Create (SKTexture texture, float /* float, not CGFloat */ alphaThreshold, CGSize size);

		[iOS(8,0), Mac(10,10)]
		[Export ("charge")]
		nfloat Charge { get; set; }

		[iOS(8,0), Mac(10,10)]
		[Export ("fieldBitMask")]
		uint FieldBitMask { get; set; } /* uint32_t */
		
		[iOS(8,0), Mac(10,10)]
		[Export ("pinned")]
		bool Pinned { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	[DisableDefaultCtor] // <quote>An SKPhysicsContact object is created automatically by Scene Kit</quote>
	partial interface SKPhysicsContact {

		[Export ("bodyA")]
		SKPhysicsBody BodyA { get; }

		[Export ("bodyB")]
		SKPhysicsBody BodyB { get; }

		[Export ("contactPoint")]
		CGPoint ContactPoint { get; }

		[Export ("collisionImpulse")]
		nfloat CollisionImpulse { get; }

		[iOS (8,0), Mac (10,10)]
		[Export ("contactNormal")]
		CGVector ContactNormal { get; }
		
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	[Model]
	[Protocol]
	partial interface SKPhysicsContactDelegate {

		[Export ("didBeginContact:")]
		void DidBeginContact (SKPhysicsContact contact);

		[Export ("didEndContact:")]
		void DidEndContact (SKPhysicsContact contact);
	}

	delegate void SKPhysicsWorldBodiesEnumeratorHandler (SKPhysicsBody body, out bool stop);
	delegate void SKPhysicsWorldBodiesAlongRayStartEnumeratorHandler (SKPhysicsBody body, CGPoint point, CGVector normal, out bool stop);

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject),
		   Delegates=new string [] {"WeakContactDelegate"},
		   Events=new Type [] {typeof (SKPhysicsContactDelegate)})]
	[DisableDefaultCtor] // <quote>You do not create SKPhysicsWorld objects directly; instead, read the physicsWorld property of an SKScene object.</quote>
	partial interface SKPhysicsWorld : NSCoding {

		[Export ("gravity")]
		CGVector Gravity { get; set; }

		[Export ("speed")]
		nfloat Speed { get; set; }

		[Export ("contactDelegate", ArgumentSemantic.Assign), NullAllowed]
		NSObject WeakContactDelegate { get; set; }

		[Wrap ("WeakContactDelegate")]
		[Protocolize]
		SKPhysicsContactDelegate ContactDelegate { get; set; }

		[Export ("addJoint:")]
		void AddJoint (SKPhysicsJoint joint);

		[Export ("removeJoint:")]
		void RemoveJoint (SKPhysicsJoint joint);

		[Export ("removeAllJoints")]
		void RemoveAllJoints ();

		[Export ("bodyAtPoint:")]
		SKPhysicsBody GetBody (CGPoint point);

		[Export ("bodyInRect:")]
		SKPhysicsBody GetBody (CGRect rect);

		[Export ("bodyAlongRayStart:end:")]
		SKPhysicsBody GetBody (CGPoint rayStart, CGPoint rayEnd);

		[Export ("enumerateBodiesAtPoint:usingBlock:")]
		void EnumerateBodies (CGPoint point, SKPhysicsWorldBodiesEnumeratorHandler enumeratorHandler);

		[Export ("enumerateBodiesInRect:usingBlock:")]
		void EnumerateBodies (CGRect rect, SKPhysicsWorldBodiesEnumeratorHandler enumeratorHandler);

		[Export ("enumerateBodiesAlongRayStart:end:usingBlock:")]
		void EnumerateBodies (CGPoint start, CGPoint end, SKPhysicsWorldBodiesAlongRayStartEnumeratorHandler enumeratorHandler);

		//
		// iOS 8
		//
		[iOS (8,0), Mac(10,10)]
		[Export ("sampleFieldsAt:")]
		[MarshalDirective (NativePrefix = "xamarin_simd__", Library = "__Internal")]
		Vector3 SampleFields (/* vector_float3 */ Vector3 position);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (NSObject))]
	[Abstract] // <quote>You never instantiate objects of this class directly</quote>
	partial interface SKPhysicsJoint : NSCoding {

		[Export ("bodyA", ArgumentSemantic.Retain)]
		SKPhysicsBody BodyA { get; set; }

		[Export ("bodyB", ArgumentSemantic.Retain)]
		SKPhysicsBody BodyB { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("reactionForce")]
		CGVector ReactionForce { get; }

		[iOS (8,0), Mac (10,10)]
		[Export ("reactionTorque")]
		nfloat ReactionTorque { get; }		
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKPhysicsJoint))]
	[DisableDefaultCtor] // impossible to set the `anchor` using the default ctor (see #14511) 
	partial interface SKPhysicsJointPin {

		[Static, Export ("jointWithBodyA:bodyB:anchor:")]
		SKPhysicsJointPin Create (SKPhysicsBody bodyA, SKPhysicsBody bodyB, CGPoint anchor);

		[Export ("shouldEnableLimits")]
		bool ShouldEnableLimits { get; set; }

		[Export ("lowerAngleLimit")]
		nfloat LowerAngleLimit { get; set; }

		[Export ("upperAngleLimit")]
		nfloat UpperAngleLimit { get; set; }

		[Export ("frictionTorque")]
		nfloat FrictionTorque { get; set; }

		[iOS (8,0), Mac (10,10)]
		[Export ("rotationSpeed")]
		nfloat RotationSpeed { get; set; }		
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKPhysicsJoint))]
	[DisableDefaultCtor] // impossible to set the `anchorA` and `anchorB` using the default ctor (see #14511) 
	partial interface SKPhysicsJointSpring {

		[Static, Export ("jointWithBodyA:bodyB:anchorA:anchorB:")]
		SKPhysicsJointSpring Create (SKPhysicsBody bodyA, SKPhysicsBody bodyB, CGPoint anchorA, CGPoint anchorB);

		[Export ("damping")]
		nfloat Damping { get; set; }

		[Export ("frequency")]
		nfloat Frequency { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKPhysicsJoint))]
	[DisableDefaultCtor] // https://bugzilla.xamarin.com/show_bug.cgi?id=14511
	partial interface SKPhysicsJointFixed {

		[Static, Export ("jointWithBodyA:bodyB:anchor:")]
		SKPhysicsJointFixed Create (SKPhysicsBody bodyA, SKPhysicsBody bodyB, CGPoint anchor);
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKPhysicsJoint))]
	[DisableDefaultCtor] // impossible to set the `anchor` and `axis` using the default ctor (see #14511) 
	partial interface SKPhysicsJointSliding {

		[Static, Export ("jointWithBodyA:bodyB:anchor:axis:")]
		SKPhysicsJointSliding Create (SKPhysicsBody bodyA, SKPhysicsBody bodyB, CGPoint anchor, CGVector axis);

		[Export ("shouldEnableLimits")]
		bool ShouldEnableLimits { get; set; }

		[Export ("lowerDistanceLimit")]
		nfloat LowerDistanceLimit { get; set; }

		[Export ("upperDistanceLimit")]
		nfloat UpperDistanceLimit { get; set; }
	}

	[Mac (10,9, onlyOn64 : true)]
	[Since (7,0)]
	[BaseType (typeof (SKPhysicsJoint))]
	[DisableDefaultCtor] // impossible to set the `anchorA` and `anchorB` using the default ctor (see #14511) 
	partial interface SKPhysicsJointLimit {

		[Export ("maxLength")]
		nfloat MaxLength { get; set; }

		[Static, Export ("jointWithBodyA:bodyB:anchorA:anchorB:")]
		SKPhysicsJointLimit Create (SKPhysicsBody bodyA, SKPhysicsBody bodyB, CGPoint anchorA, CGPoint anchorB);
	}

	[Mac (10,10, onlyOn64 : true)]
	[iOS (8,0)]
	[BaseType (typeof (NSObject))]
	interface SKRange : NSCoding, NSCopying {
		[DesignatedInitializer]
		[Export ("initWithLowerLimit:upperLimit:")]
		IntPtr Constructor (nfloat lowerLimit, nfloat upperLimier);

		[Export ("lowerLimit")]
		nfloat LowerLimit { get; set; }

		[Export ("upperLimit")]
		nfloat UpperLimit { get; set; }

		[Static, Export ("rangeWithLowerLimit:upperLimit:")]
		SKRange Create (nfloat lower, nfloat upper);

		[Static, Export ("rangeWithLowerLimit:")]
		SKRange CreateWithLowerLimit (nfloat lower);

		[Static, Export ("rangeWithUpperLimit:")]
		SKRange CreateWithUpperLimit (nfloat upper);

		[Static, Export ("rangeWithConstantValue:")]
		SKRange CreateConstant (nfloat value);

		[Static, Export ("rangeWithValue:variance:")]
		SKRange CreateWithVariance (nfloat value, nfloat variance);

		[Static, Export ("rangeWithNoLimits")]
		SKRange CreateUnlimited ();
	}

	[iOS (9,0)]
	[Mac (10,11, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	[DisableDefaultCtor]
	interface SKAudioNode : NSCoding {
		[Export ("initWithAVAudioNode:")]
		[DesignatedInitializer]
		IntPtr Constructor ([NullAllowed] AVAudioNode node);

		[Export ("initWithFileNamed:")]
		IntPtr Constructor (string fileName);

		[Export ("initWithURL:")]
		IntPtr Constructor (NSUrl url);

		[NullAllowed, Export ("avAudioNode", ArgumentSemantic.Retain)]
		AVAudioNode AvAudioNode { get; set; }

		[Export ("autoplayLooped")]
		bool AutoplayLooped { get; set; }

		[Export ("positional")]
		bool Positional { [Bind ("isPositional")] get; set; }
	}

	[iOS (9,0)]
	[Mac (10,11, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	interface SKCameraNode {
		[Export ("containsNode:")]
		bool Contains (SKNode node);

		[Export ("containedNodeSet")]
		NSSet<SKNode> ContainedNodeSet { get; }
	}

	[iOS (9,0)]
	[Mac (10,11, onlyOn64 : true)]
	[BaseType (typeof (SKNode))]
	[DisableDefaultCtor]
	interface SKReferenceNode {
		[Export ("initWithURL:")]
		[DesignatedInitializer]
		IntPtr Constructor ([NullAllowed] NSUrl url);

		[Export ("initWithFileNamed:")]
		[DesignatedInitializer]
		IntPtr Constructor ([NullAllowed] string fileName);

		[Static]
		[Export ("referenceNodeWithFileNamed:")]
		SKReferenceNode FromFile (string fileName);

		[Static]
		[Export ("referenceNodeWithURL:")]
		SKReferenceNode FromUrl (NSUrl referenceUrl);

		[Export ("didLoadReferenceNode:")]
		void DidLoadReferenceNode ([NullAllowed] SKNode node);

		[Export ("resolveReferenceNode")]
		void Resolve ();
	}
}
#endif
