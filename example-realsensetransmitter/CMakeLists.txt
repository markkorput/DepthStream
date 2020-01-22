cmake_minimum_required(VERSION 3.10)
project(RealSenseTransmitter VERSION 0.1.0 LANGUAGES CXX)

add_executable(RealSenseTransmitter src/main.cpp)
target_compile_features(RealSenseTransmitter PRIVATE cxx_auto_type)

find_package(DepthStream 0.1.0 REQUIRED)
find_package(realsense2 REQUIRED)
find_package(ZLIB)
target_link_libraries(RealSenseTransmitter DepthStream::DepthStream realsense2::realsense2)



# include(CheckCXXCompilerFlag)
# CHECK_CXX_COMPILER_FLAG("-std=c++11" COMPILER_SUPPORTS_CXX11)
# CHECK_CXX_COMPILER_FLAG("-std=c++0x" COMPILER_SUPPORTS_CXX0X)
# if(COMPILER_SUPPORTS_CXX11)
#     set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")
# elseif(COMPILER_SUPPORTS_CXX0X)
#     set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++0x")
# endif()


# ## RPATH STUFF TAKEN FROM:
# ## https://gitlab.kitware.com/cmake/community/wikis/doc/cmake/RPATH-handling

# # use, i.e. don't skip the full RPATH for the build tree
# SET(CMAKE_SKIP_BUILD_RPATH  FALSE)

# # when building, don't use the install RPATH already
# # (but later on when installing)
# SET(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)

# SET(CMAKE_INSTALL_RPATH "${CMAKE_INSTALL_PREFIX}/lib")

# # add the automatically determined parts of the RPATH
# # which point to directories outside the build tree to the install RPATH
# # SET(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)


# # the RPATH to be used when installing, but only if it's not a system directory
# LIST(FIND CMAKE_PLATFORM_IMPLICIT_LINK_DIRECTORIES "${CMAKE_INSTALL_PREFIX}/lib" isSystemDir)
# IF("${isSystemDir}" STREQUAL "-1")
#    SET(CMAKE_INSTALL_RPATH "${CMAKE_INSTALL_PREFIX}/lib")
# ENDIF("${isSystemDir}" STREQUAL "-1")


# find_package(realsense2 REQUIRED)
# find_package(DepthStream REQUIRED)
# find_package(ZLIB)

# get_filename_component( APP_src "${CMAKE_CURRENT_LIST_DIR}/src" ABSOLUTE )
# FILE(GLOB APP_sources ${APP_src}/*.cpp)
# # file(GLOB DEPTH_STREAM_SOURCES "../src/*.cpp")

# # add_executable(RealSenseTransmitter src/RealSenseTransmitter.cpp ${DEPTH_STREAM_SOURCES})
# add_executable(RealSenseTransmitter ${APP_sources})
# target_link_libraries(RealSenseTransmitter PUBLIC ${ZLIB_LIBRARIES} PRIVATE realsense2::realsense2 PRIVATE DepthStream)

