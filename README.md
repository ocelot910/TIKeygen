# TIKeygen 
License generator for TI Nspire CX CAS Student Software  
## Features
- Activated until 2038 ([Y2K38 problem](https://en.wikipedia.org/wiki/Year_2038_problem))
- Only requires copying a file
- Doesn't require administrator rights
- Works on Windows, MacOS, and Linux
## How to use
1) Download and install the appropriate version of the TI Nspire CX CAS Student Software:
   - [Windows](https://education.ti.com/en/software/details/en/36BE84F974E940C78502AA47492887AB/ti-nspirecxcas_pc_full)
   - [MacOS - v6.2](https://tiplanet.org/forum/archives_voir.php?id=4585176) - v6.3 doesn't work on M-series MacOS regardless of whether you have a valid license or not
   - [Linux](https://education.ti.com/en/software/details/en/36BE84F974E940C78502AA47492887AB/ti-nspirecxcas_pc_full) - Install via Wine
3) Go to this [page](https://ocelot910.github.io/TIKeygen/) and follow the instructions
## How it works
- When the software starts, it first checks if a `license.properties` file exists in the `actdata` folder. If it does, then the software validates it.
- If the file is deemed valid, the software then decrypts the `license_token` payload with the public key provided in the `n` field of the `license.properties` file.  
- This `license_token` payload contains the token expiration date and the subscription expiration date,
   - You can set these two fields to the maximum possible value being 2,147,483,647 (2038-01-19), which is the number of seconds after January 1, 1970.  
   - It also has a field named `jti` that supposedly contains your HWID. This HWID is calculated by adding your machine name to your username then converting its MD5 hash into a GUID  
- Interestingly, the software only fetches and validates the public key from the TI Nspire website against the local `license.properties` file only if the token has expired, so as long the current time is earlier than the token expiration date, this step is skipped, allowing any public key to be accepted.
