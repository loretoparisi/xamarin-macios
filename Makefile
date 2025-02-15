TOP=.
SUBDIRS=builds runtime fsharp src tools msbuild
include $(TOP)/Make.config
include $(TOP)/mk/versions.mk

#
# Xamarin.iOS
#

IOS_DIRECTORIES += \
	$(IOS_DESTDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions \
	$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX) \
	$(IOS_DESTDIR)/Developer/MonoTouch \
	$(IOS_DESTDIR)/Developer/MonoTouch/usr \
	$(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono \

IOS_TARGETS += \
	$(IOS_INSTALL_DIRECTORIES) \
	$(IOS_DESTDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current \
	$(IOS_DESTDIR)/Developer/MonoTouch/usr/bin \
	$(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono/2.1 \
	$(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono/Xamarin.iOS \
	$(IOS_DESTDIR)/Developer/MonoTouch/updateinfo \
	$(IOS_DESTDIR)/Developer/MonoTouch/Version \
	$(IOS_DESTDIR)/Developer/MonoTouch/usr/share \
	$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/buildinfo \
	$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/Version \
	$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/updateinfo \

$(IOS_DESTDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current: | $(IOS_DESTDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions
	$(Q_LN) ln -hfs $(IOS_INSTALL_VERSION) $@

$(IOS_DESTDIR)/Developer/MonoTouch/usr/bin: | $(IOS_DESTDIR)/Developer/MonoTouch/usr
	$(Q_LN) ln -Fs $(abspath $(IOS_TARGETDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/bin) $@

$(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono/2.1: | $(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono
	$(Q_LN) ln -Fs $(abspath $(IOS_TARGETDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/2.1) $@

$(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono/Xamarin.iOS: | $(IOS_DESTDIR)/Developer/MonoTouch/usr/lib/mono
	$(Q_LN) ln -Fs $(abspath $(IOS_TARGETDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/lib/mono/Xamarin.iOS) $@

$(IOS_DESTDIR)/Developer/MonoTouch/updateinfo: | $(IOS_DESTDIR)/Developer/MonoTouch
	$(Q_LN) ln -fs $(abspath $(IOS_TARGETDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/updateinfo) $@

$(IOS_DESTDIR)/Developer/MonoTouch/Version: | $(IOS_DESTDIR)/Developer/MonoTouch
	$(Q_LN) ln -fs $(abspath $(IOS_TARGETDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/Version) $@

$(IOS_DESTDIR)/Developer/MonoTouch/usr/share: | $(IOS_DESTDIR)/Developer/MonoTouch/usr
	$(Q_LN) ln -Fs $(abspath $(IOS_TARGETDIR)/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/share) $@

$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/buildinfo: Make.config .git/index | $(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)
	$(Q_GEN) echo "Version: $(IOS_PACKAGE_VERSION)" > $@
	$(Q) echo "Hash: $(shell git log --oneline -1 --pretty=%h)" >> $@
	$(Q) echo "Branch: $(CURRENT_BRANCH)" >> $@
	$(Q) echo "Build date: $(shell date '+%Y-%m-%d %H:%M:%S%z')" >> $@

$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/Version: Make.config
	$(Q) echo $(IOS_PACKAGE_VERSION) > $@

$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/updateinfo: Make.config
	$(Q) echo "4569c276-1397-4adb-9485-82a7696df22e $(IOS_PACKAGE_UPDATE_ID)" > $@

ifdef INCLUDE_IOS
TARGETS += $(IOS_TARGETS)
DIRECTORIES += $(IOS_DIRECTORIES)
endif

#
# Xamarin.Mac
#

MAC_DIRECTORIES += \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Versions \
	$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR) \

MAC_TARGETS += \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Versions/Current \
	$(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Commands \
	$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/buildinfo \
	$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/Version \
	$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/updateinfo \

$(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Versions/Current: | $(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Versions
	$(Q_LN) ln -hfs $(MAC_INSTALL_VERSION) $@

$(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Commands:
	$(Q_LN) ln -hfs $(MAC_TARGETDIR)/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/bin $@

$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/buildinfo: Make.config .git/index | $(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)
	$(Q_GEN) echo "Version: $(MAC_PACKAGE_VERSION)" > $@
	$(Q) echo "Hash: $(shell git log --oneline -1 --pretty=%h)" >> $@
	$(Q) echo "Branch: $(shell git symbolic-ref --short HEAD)" >> $@
	$(Q) echo "Build date: $(shell date '+%Y-%m-%d %H:%M:%S%z')" >> $@

$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/updateinfo: Make.config
	$(Q) echo "0ab364ff-c0e9-43a8-8747-3afb02dc7731 $(MAC_PACKAGE_UPDATE_ID)" > $@

$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/Version: Make.config
	$(Q) echo $(MAC_PACKAGE_VERSION) > $@

ifdef INCLUDE_MAC
TARGETS += $(MAC_TARGETS)
DIRECTORIES += $(MAC_DIRECTORIES)
endif

#
# Common
#

.PHONY: world
world: check-system
	@$(MAKE) reset-versions
	@$(MAKE) all -j8
	@$(MAKE) install -j8

.PHONY: check-system
check-system:
	@./system-dependencies.sh

$(DIRECTORIES):
	$(Q) mkdir -p $@

$(TARGETS): | check-system

all-local:: $(TARGETS)
install-local:: $(TARGETS)

install-hook::
ifdef INCLUDE_IOS
ifneq ($(findstring $(IOS_DESTDIR)$(MONOTOUCH_PREFIX),$(shell ls -l /Library/Frameworks/Xamarin.iOS.framework/Versions/Current 2>&1)),)
	@echo
	@echo "	This build of Xamarin.iOS is the now default version on your system. "
	@echo
else
	@echo
	@echo "	Xamarin.iOS has not been installed into your system by 'make install'"
	@echo "	In order to set the currently built Xamarin.iOS as your system version,"
	@echo "	execute 'make install-system'".
	@echo
endif
endif
ifdef INCLUDE_MAC
ifndef INCLUDE_IOS
	@echo
endif
ifneq ($(findstring $(abspath $(MAC_DESTDIR)$(MAC_FRAMEWORK_DIR)/Versions),$(shell ls -l $(MAC_FRAMEWORK_DIR)/Versions/Current 2>&1)),)
	@echo "	This build of Xamarin.Mac is the now default version on your system. "
	@echo
else
	@echo "	Xamarin.Mac has not been installed into your system by 'make install'"
	@echo "	In order to set the currently built Xamarin.Mac as your system version,"
	@echo "	execute 'make install-system'".
	@echo
endif
endif

install-system: install-system-ios install-system-mac

install-system-ios:
	@if ! test -s "$(IOS_DESTDIR)/$(MONOTOUCH_PREFIX)/buildinfo"; then echo "The Xamarin.iOS build seems incomplete. Did you run \"make install\"?"; exit 1; fi
	$(Q) rm -f /Library/Frameworks/Xamarin.iOS.framework/Versions/Current
	$(Q) mkdir -p /Library/Frameworks/Xamarin.iOS.framework/Versions
	$(Q) ln -s $(IOS_DESTDIR)$(MONOTOUCH_PREFIX) /Library/Frameworks/Xamarin.iOS.framework/Versions/Current
	$(Q) echo Installed Xamarin.iOS into /Library/Frameworks/Xamarin.iOS.framework/Versions/Current

install-system-mac:
	@if ! test -s "$(MAC_DESTDIR)/$(MAC_FRAMEWORK_CURRENT_DIR)/buildinfo" ; then echo "The Xamarin.Mac build seems incomplete. Did you run \"make install\"?"; exit 1; fi
	$(Q) rm -f $(MAC_FRAMEWORK_DIR)/Versions/Current
	$(Q) mkdir -p $(MAC_FRAMEWORK_DIR)/Versions
	$(Q) ln -s $(MAC_DESTDIR)$(MAC_FRAMEWORK_CURRENT_DIR) $(MAC_FRAMEWORK_DIR)/Versions/Current
	$(Q) echo Installed Xamarin.Mac into $(MAC_FRAMEWORK_DIR)/Versions/Current

fix-install-permissions:
	sudo mkdir -p /Library/Frameworks/Mono.framework/External/
	sudo mkdir -p /Library/Frameworks/Xamarin.iOS.framework
	sudo mkdir -p /Library/Frameworks/Xamarin.Mac.framework
	sudo chown -R $(USER) /Library/Frameworks/Mono.framework/External/
	sudo chown -R $(USER) /Library/Frameworks/Xamarin.iOS.framework
	sudo chown -R $(USER) /Library/Frameworks/Xamarin.Mac.framework

ifdef ENABLE_XAMARIN
SUBDIRS += $(MACCORE_PATH)
endif
