cmake_minimum_required(VERSION 3.10)
project(DepthStreamDumpOsc VERSION 0.1.0 LANGUAGES CXX)

add_executable(DepthStreamDumpOsc src/main.cpp)
target_compile_features(DepthStreamDumpOsc PRIVATE cxx_auto_type)

# find_package(DepthStream 0.1.1 REQUIRED)
include(${CMAKE_CURRENT_LIST_DIR}/../CMakeLists.txt)
target_link_libraries(DepthStreamDumpOsc DepthStream::DepthStream)
