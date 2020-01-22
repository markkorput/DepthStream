cmake_minimum_required(VERSION 3.0.0)
project(DepthStream VERSION 0.1.0)

include(CheckCXXCompilerFlag)
CHECK_CXX_COMPILER_FLAG("-std=c++11" COMPILER_SUPPORTS_CXX11)
CHECK_CXX_COMPILER_FLAG("-std=c++0x" COMPILER_SUPPORTS_CXX0X)
if(COMPILER_SUPPORTS_CXX11)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")
elseif(COMPILER_SUPPORTS_CXX0X)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++0x")
endif()

include(CTest)
enable_testing()

add_library(DepthStream src/Transmitter.cpp src/CamInterface.cpp src/Compressor.cpp src/Inflater.cpp src/OniSampleUtilities.cpp src/Playback.cpp src/Receiver.cpp src/Recorder.cpp src/Windowsstuff.cpp src/functional.cpp)

set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)
