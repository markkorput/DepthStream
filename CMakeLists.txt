# this file is inspired by https://github.com/pabloariasal/modern-cmake-sample/blob/master/libjsonutils/CMakeLists.txt
cmake_minimum_required(VERSION 3.10)
project(DepthStream VERSION 0.1.1)

##############################################
# Declare dependencies

find_package(ZLIB)

##############################################
# Create target and set properties

# source files
get_filename_component( DepthStream_src "${CMAKE_CURRENT_LIST_DIR}/src" ABSOLUTE )
FILE(GLOB DepthStream_sources ${DepthStream_src}/*.cpp)

add_library(DepthStream ${DepthStream_sources})

#Add an alias so that library can be used inside the build tree, e.g. when testing
add_library(DepthStream::DepthStream ALIAS DepthStream)

target_include_directories(DepthStream
    PUBLIC 
        $<INSTALL_INTERFACE:include>
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    # PRIVATE
    #     ${CMAKE_CURRENT_SOURCE_DIR}/src
)

# specify the C++ standard
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED True)

target_compile_features(DepthStream PRIVATE cxx_auto_type)

target_link_libraries(DepthStream
    # PUBLIC
    #     Boost::boost RapidJSON::RapidJSON
    # PRIVATE
    #     Boost::regex
    PRIVATE
      ${ZLIB_LIBRARIES}
)


##############################################
# Installation instructions

include(GNUInstallDirs)
set(INSTALL_CONFIGDIR ${CMAKE_INSTALL_LIBDIR}/cmake/DepthStream)
install(TARGETS DepthStream
    EXPORT depthstream-targets
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
)

#This is required so that the exported target has the name JSONUtils and not jsonutils
set_target_properties(DepthStream PROPERTIES EXPORT_NAME DepthStream)

install(DIRECTORY include/ DESTINATION ${CMAKE_INSTALL_INCLUDEDIR})

#Export the targets to a script
install(EXPORT depthstream-targets
  FILE
    DepthStreamTargets.cmake
  NAMESPACE
    DepthStream::
  DESTINATION
    ${INSTALL_CONFIGDIR}
)

#Create a ConfigVersion.cmake file
include(CMakePackageConfigHelpers)
write_basic_package_version_file(
    ${CMAKE_CURRENT_BINARY_DIR}/DepthStreamConfigVersion.cmake
    VERSION ${PROJECT_VERSION}
    COMPATIBILITY AnyNewerVersion
)

configure_package_config_file(${CMAKE_CURRENT_LIST_DIR}/cmake/DepthStreamConfig.cmake.in
    ${CMAKE_CURRENT_BINARY_DIR}/DepthStreamConfig.cmake
    INSTALL_DESTINATION ${INSTALL_CONFIGDIR}
)

#Install the config, configversion and custom find modules
install(FILES
    # ${CMAKE_CURRENT_LIST_DIR}/cmake/FindRapidJSON.cmake
    ${CMAKE_CURRENT_BINARY_DIR}/DepthStreamConfig.cmake
    ${CMAKE_CURRENT_BINARY_DIR}/DepthStreamConfigVersion.cmake
    DESTINATION ${INSTALL_CONFIGDIR}
)

##############################################
## Exporting from the build tree
# configure_file(${CMAKE_CURRENT_LIST_DIR}/cmake/FindRapidJSON.cmake
#     ${CMAKE_CURRENT_BINARY_DIR}/FindRapidJSON.cmake
#     COPYONLY)

export(EXPORT depthstream-targets FILE ${CMAKE_CURRENT_BINARY_DIR}/DepthStreamTargets.cmake NAMESPACE DepthStream::)

#Register package in user's package registry
export(PACKAGE DepthStream)

# add_subdirectory(test)

##############################################
# CPack / Testing

set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)

include(CTest)
enable_testing()
