#import <Foundation/Foundation.h>
#include <objc/objc.h>
#include <objc/runtime.h>
#include <objc/message.h>

#include "xamarin/xamarin.h"
#include "monotouch-support.h"

const char *
xamarin_get_locale_country_code ()
{
	NSLocale *locale = [NSLocale currentLocale];
	NSString *cc = [locale objectForKey: NSLocaleCountryCode];
	return strdup ([cc UTF8String]);
}

void
xamarin_log (const unsigned short *unicodeMessage)
{
	NSLog (@"%S", unicodeMessage); 
}

void*
xamarin_timezone_get_data (const char *name, int *size)
{
	NSTimeZone *tz = nil;
	if (name) {
		NSString *n = [[NSString alloc] initWithUTF8String: name];
		tz = [[NSTimeZone alloc] initWithName:n];
		[n release];
	} else {
		tz = [NSTimeZone localTimeZone];
	}
	NSData *data = [tz data];
	*size = [data length];
	void* result = malloc (*size);
	memcpy (result, data.bytes, *size);
	[tz release];
	return result;
}

char**
xamarin_timezone_get_names (int *count)
{
	NSArray *array = [NSTimeZone knownTimeZoneNames];
	*count = array.count;
	char** result = (char**) malloc (sizeof (char*) * (*count));
	for (int i = 0; i < *count; i++) {
		NSString *s = [array objectAtIndex: i];
		result [i] = strdup (s.UTF8String);
	}
	return result;
}

#if !TARGET_OS_WATCH && !TARGET_OS_TV
void
xamarin_start_wwan (const char *uri)
{
#if defined(__i386__) || defined (__x86_64__)
	return;
#else
	unsigned char buf[1];
	CFStringRef host = CFStringCreateWithCString (kCFAllocatorDefault, uri, kCFStringEncodingUTF8);
	CFStringRef get = CFStringCreateWithCString (kCFAllocatorDefault, "GET", kCFStringEncodingUTF8);
	CFURLRef url = CFURLCreateWithString (kCFAllocatorDefault, host, nil);
	
	CFHTTPMessageRef message = CFHTTPMessageCreateRequest (kCFAllocatorDefault, get, url, kCFHTTPVersion1_1);
	CFReadStreamRef stream = CFReadStreamCreateForHTTPRequest (kCFAllocatorDefault, message);
	
	CFReadStreamScheduleWithRunLoop (stream, CFRunLoopGetCurrent (), kCFRunLoopCommonModes);
	
	if (CFReadStreamOpen (stream)) {
		// CFStreamStatus status = CFReadStreamGetStatus (stream);
		// NSLog (@"CFStreamStatus %i", status);
		// note: some earlier iOS7 beta returned 1 (Opening) instead of 2 (Open) - a bit more time was needed or
		// CFReadStreamRead blocks (and never return)
		CFReadStreamRead (stream, buf, 1);
	}
	// that will remove it from the runloop (so we do it even if open failed)
	CFReadStreamClose (stream);
	
	CFRelease (stream);
	CFRelease (host);
	CFRelease (get);
	CFRelease (url);
	CFRelease (message);
#endif
}
#endif /* !TARGET_OS_WATCH && !TARGET_OS_TV */

#if defined (MONOTOUCH)
// called from mono-extensions/mcs/class/corlib/System/Environment.iOS.cs
const char *
xamarin_GetFolderPath (int folder)
{
	// NSUInteger-based enum (and we do not want corlib exposed to 32/64 bits differences)
	NSSearchPathDirectory dd = (NSSearchPathDirectory) folder;
	NSURL *url = [[[NSFileManager defaultManager] URLsForDirectory:dd inDomains:NSUserDomainMask] lastObject];
	NSString *path = [url path];
	return strdup ([path UTF8String]);
}
#endif /* defined (MONOTOUCH) */

#if TARGET_OS_TV && defined (__arm64__)

// there are no objc_msgSend[Super]_stret functions on ARM64 but the managed
// code might have (e.g. linker is off) references to the symbol, which makes
// it impossible to disable dlsym and, for example, run dontlink on devices
// https://bugzilla.xamarin.com/show_bug.cgi?id=36569#c4

void objc_msgSend_stret (id self, SEL op, ...)
{
	NSLog (@"Unimplemented objc_msgSend_stret %s", sel_getName (op));
	abort ();
}

void objc_msgSendSuper_stret (struct objc_super *super, SEL op, ...)
{
	NSLog (@"Unimplemented objc_msgSendSuper_stret %s", sel_getName (op));
	abort ();
}

#endif

