# ImageOptimizer

> A C# benchmark tool that compares **sequential** vs **parallel** image processing performance, generating multiple WebP resolutions from local files or remote URLs.

---

## Table of Contents

- [Overview](#-overview)
- [Design patterns used](#-design-patterns-used)
- [Project Structure](#-project-structure)
- [Usage](#-usage)
- [How It Works](#-how-it-works)
- [Optimizations Implemented](#-optimizations-implemented)
- [Benchmark Results](#-benchmark-results)
- [Dependencies](#-dependencies)

---

## Overview

**ImageOptimizer** is a console application that processes images by generating multiple WebP resolutions (1080p, 720p, 480p) from a single source. Its primary purpose is to **demonstrate and measure the performance gain of parallelism** over sequential execution on two distinct workload types:

- **CPU-bound** workload (local image resizing)
- **Mixed I/O + CPU** workload (download + resize from remote URLs)

---

## Design patterns used

- **Strategy pattern** — `IImageProcessorService` allows swapping execution strategies without changing calling code
- **Dependency injection** (manual) — `HttpClient` and `ImageDownloaderService` are injected into processors
- **Optional dependencies** — processors accept a nullable downloader; FOLDER mode skips HTTP setup entirely

---

## Project Structure

```
ImageOptimizer/
├── Program.cs                          # Entry point, UI, orchestration
├── ImageOptimizer.csproj               # Project file & dependencies
│
├── Models/
│   ├── ImageModel.cs                   # DTO for JSON sources (Name + Url)
│   └── Enums/
│       └── ModeEnum.cs                 # FILE / FOLDER mode enumeration
│
├── Services/
│   ├── IImageProcessorService.cs       # Strategy interface
│   ├── SequentialImageProcessor.cs     # Sequential implementation
│   ├── ParallelImageProcessor.cs       # Parallel implementation
│   ├── ImageDownloaderService.cs       # HTTP download wrapper
│   └── ImageResizerService.cs          # ImageSharp resize logic
│
├── Datas/
│   ├── Images/                         # (FOLDER mode) source images
│   ├── Outputs/                        # Generated WebP files
└   └── images.json                     # (FILE mode) URL sources
```

---

## Usage

When you launch the application, it will prompt you for:

1. **Input path** — either a folder of images (FOLDER mode) or a `.json` file (FILE mode)
2. **Output directory** — where the generated WebP files will be saved

### FOLDER mode (MVP)

Process all `.jpg`, `.jpeg`, and `.png` images from a local folder.

```
Input path : ./Images
Output directory : ./Outputs
```

### FILE mode (V1)

Download images from a list of URLs and process them.

```
Input path : ./images.json
Output directory : ./Outputs
```

#### Example `images.json`

```json
[
  { "Name": "mountain", "Url": "https://images.unsplash.com/photo-1506905925346-21bda4d32df4" },
  { "Name": "forest",   "Url": "https://images.unsplash.com/photo-1441974231531-c6227db76b6e" },
  { "Name": "ocean",    "Url": "https://images.unsplash.com/photo-1505142468610-359e7d316be0" }
]
```

### Output structure

```
Outputs/
├── SequentialImages/
│   ├── mountain_1080p.webp
│   ├── mountain_720p.webp
│   ├── mountain_480p.webp
│   └── ...
└── ParallelImages/
    └── ... (same files, generated in parallel)
```

---

## How It Works

### 1. Mode detection

The application inspects the input path and decides automatically:
- Is it a directory? → **FOLDER mode**
- Is it a `.json` file? → **FILE mode**
- Otherwise → error and exit

### 2. Service initialization

In FOLDER mode, only the resizer is needed (no network). In FILE mode, an `HttpClient` and an `ImageDownloaderService` are added to the pipeline.

### 3. Image processing pipeline

For each source image (whether local or downloaded):

```
[Source] → [Load into ImageSharp] → [Clone × 3 resolutions] → [Encode WebP] → [Save to disk]
```

The image is **loaded once** then **cloned for each target resolution** to avoid re-decoding the source three times.

### 4. Benchmarking

Both strategies (sequential and parallel) run on the same workload, with `Stopwatch` measuring elapsed time. The speedup factor is computed as `sequentialTime / parallelTime`.

---

## Optimizations Implemented

This is the heart of the project. Here are the optimizations applied to the **parallel version**, and why they matter.

### 1. `Parallel.ForEachAsync` instead of `Parallel.ForEach`

**Why?** `Parallel.ForEach` is built for **synchronous** workloads. Using it with async code forces blocking on `Task.Wait()` or `.Result`, which wastes threads and breaks async correctness.


### 2. Controlled parallelism with `MaxDegreeOfParallelism`

Without a cap, `Parallel.ForEachAsync` could spawn dozens of tasks at once, saturating CPU and triggering HTTP rate limits or socket exhaustion. We cap at **8 concurrent operations** by default.

### 3. Combined `async/await` + parallel scheduling

In FILE mode, each task is a **mixed I/O + CPU pipeline**:
1. HTTP download (I/O-bound — thread is freed during the wait)
2. Decode + resize (CPU-bound — thread is busy)

This combination is the most efficient pattern in modern .NET:
- While task A waits for the network, task B can encode WebP
- While task C decodes a large PNG, task D can already download the next image
- The thread pool is never idle

### 4. Single load + multiple clones (ImageSharp)

For each image, we **load and decode once**, then `Clone()` the in-memory representation for each target resolution:

This avoids the cost of decoding the source 3 times (one per resolution), which can be the most expensive step for large JPEGs.

### 5. Shared `HttpClient` (singleton lifetime)

A single `HttpClient` instance is created at the application level and reused across all downloads. Creating a new `HttpClient` per request would lead to **socket exhaustion** under load.

---

## Benchmark Results

### Test machine

- **CPU:** Intel Gen Intel(R) Core(TM) i7-12700H (14 cores / 20 threads)
- **RAM:** 32 GB DDR4
- **Storage:** NVMe SSD

### FOLDER mode (CPU-bound)

| Images | Sequential | Parallel | Speedup |
|--------|-----------:|---------:|--------:|
| 5      | _10.57s_   | _3.48s_ | **x3.04** |

### FILE mode (I/O + CPU)

| URLs | Sequential | Parallel | Speedup |
|------|-----------:|---------:|--------:|
| 5    | _37.41s_   | _5.23s_ | **x7.15** |

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| **SixLabors.ImageSharp** | 3.1.5+ | Image loading, resizing, WebP encoding |
| **Spectre.Console** | latest | CLI rendering (banners, tables, spinners) |
