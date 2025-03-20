# Hey Dude
I upload some *.tfs documents, they are all bigger than 100m. I have uploaded them with LFS. So when using the scence Battlefield you need to do download LFS documents first.

## Description
This project contains large `.tif` files stored using **Git LFS (Large File Storage)**. By default, when you `clone` or `pull` this repository, you will only get **pointers** to these files, not the actual data.

To ensure you download the full `.tif` files, follow the steps below.

---

## How to Download LFS Files

### **For Command Line Users**
After cloning or pulling the repository, run:
```sh
git lfs install  # Ensure Git LFS is installed
git lfs pull     # Download the actual .tif files
