cmake_minimum_required(VERSION 3.10)
project(RealSenseTransmitter VERSION 0.1.0 LANGUAGES CXX)

add_executable(RealSenseTransmitter src/main.cpp)
target_compile_features(RealSenseTransmitter PRIVATE cxx_auto_type)

# find_package(DepthStream 0.1.1 REQUIRED)
include(${CMAKE_CURRENT_LIST_DIR}/../CMakeLists.txt)
find_package(realsense2 REQUIRED)
target_link_libraries(RealSenseTransmitter DepthStream::DepthStream realsense2::realsense2)