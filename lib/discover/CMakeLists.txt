# this file is inspired by https://github.com/pabloariasal/modern-cmake-sample/blob/master/libjsonutils/CMakeLists.txt
cmake_minimum_required(VERSION 3.10)
project(Discover VERSION 0.0.1)

##############################################
# Declare dependencies

# find_package(ZLIB)
find_package (Threads)
find_package (liblo)
include (${CMAKE_CURRENT_LIST_DIR}/cmake/Findliblo.cmake)

##############################################
# Create target and set properties

# source files
get_filename_component( Discover_src "${CMAKE_CURRENT_LIST_DIR}/src" ABSOLUTE )
FILE(GLOB Discover_sources ${Discover_src}/*.cpp ${Discover_src}/*/*.cpp)

add_library(Discover ${Discover_sources})

#Add an alias so that library can be used inside the build tree, e.g. when testing
add_library(Discover::Discover ALIAS Discover)

target_include_directories(Discover
    PUBLIC 
      $<INSTALL_INTERFACE:include>
      $<BUILD_INTERFACE:${CMAKE_CURRENT_LIST_DIR}/include>
    PRIVATE
      ${LIBLO_INCLUDE_DIRS}
      #${CMAKE_CURRENT_LIST_DIR}/src
)

# specify the C++ standard
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED True)

target_compile_features(Discover PRIVATE cxx_auto_type)

target_link_libraries(Discover
    # PUBLIC
    #     Boost::boost RapidJSON::RapidJSON
    # PRIVATE
    #     Boost::regex
    PRIVATE
      # ${ZLIB_LIBRARIES}
      ${CMAKE_THREAD_LIBS_INIT}
      ${LIBLO_LIBRARIES}
)


##############################################
# Installation instructions

include(GNUInstallDirs)
set(INSTALL_CONFIGDIR ${CMAKE_INSTALL_LIBDIR}/cmake/Discover)
install(TARGETS Discover
    EXPORT discover-targets
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
)

#This is required so that the exported target has the name JSONUtils and not jsonutils
set_target_properties(Discover PROPERTIES EXPORT_NAME Discover)

install(DIRECTORY include/ DESTINATION ${CMAKE_INSTALL_INCLUDEDIR})

#Export the targets to a script
install(EXPORT discover-targets
  FILE
    DiscoverTargets.cmake
  NAMESPACE
    Discover::
  DESTINATION
    ${INSTALL_CONFIGDIR}
)

#Create a ConfigVersion.cmake file
include(CMakePackageConfigHelpers)
write_basic_package_version_file(
    ${CMAKE_CURRENT_BINARY_DIR}/DiscoverConfigVersion.cmake
    VERSION ${PROJECT_VERSION}
    COMPATIBILITY AnyNewerVersion
)

configure_package_config_file(${CMAKE_CURRENT_LIST_DIR}/cmake/DiscoverConfig.cmake.in
    ${CMAKE_CURRENT_BINARY_DIR}/DiscoverConfig.cmake
    INSTALL_DESTINATION ${INSTALL_CONFIGDIR}
)

#Install the config, configversion and custom find modules
install(FILES
    # ${CMAKE_CURRENT_LIST_DIR}/cmake/FindRapidJSON.cmake
    ${CMAKE_CURRENT_BINARY_DIR}/DiscoverConfig.cmake
    ${CMAKE_CURRENT_BINARY_DIR}/DiscoverConfigVersion.cmake
    DESTINATION ${INSTALL_CONFIGDIR}
)

##############################################
## Exporting from the build tree
# configure_file(${CMAKE_CURRENT_LIST_DIR}/cmake/FindRapidJSON.cmake
#     ${CMAKE_CURRENT_BINARY_DIR}/FindRapidJSON.cmake
#     COPYONLY)

export(EXPORT discover-targets FILE ${CMAKE_CURRENT_BINARY_DIR}/DiscoverTargets.cmake NAMESPACE Discover::)

#Register package in user's package registry
export(PACKAGE Discover)

# add_subdirectory(test)

##############################################
# CPack / Testing

set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)

include(CTest)
enable_testing()
