# CloudCoinCore-2.0
CloudCoinCore Version 2.0 With Performance and structural enhancements


Here are the proposed Changes:
NOTE THE MOST IMPORTANT ARE THE TOP THREE

* 1. Help button. This button would show the Help Desk's contact info; URL to CloudCoinwiki, FAQs, Links to video page.

* 2. Secondary DNS support. This would allow for us to hardcode a second DNS address that the client could check and use if it is faster than the CloudFlare. However, we are still deciding on the final implementation of this. 

* 3. Cloud Collectables support. When exporting, the client will check with the Collectable's server to see if it has a custome JPEG. If it does it will get a ticket from the fasted RAIDA and take it to the Collectables serer. The Collectables server then give the client the jpeg. 

4. Support new file formats besides JPEG and .STACK . Specifically supporting QR codes that would be displayed and printed out. 

5. Support for Store-in-mind (but we need to figure out a better way) perhaps. 

6. Check writing and Check cashing abilities to work with CloudBank.

7. List all serial number in a folder.

8. Export for sale 

9. International (supports multi languages).

10. For mobile apps, support for recovering exported coins. 

11. Counterfeit coins get a txt file explaining why they were counterfeit. 

12. Support for Splitting (multiplying of coins)

13. Contrast (Change the way it looks so blind people can see)

14. Automatic Updates and website downloads. Gitinsky knows how to do this. 

15. Lost Coin Report Support

16. Flexable jpegs. Allow jpegs to be dropped into template folder. (This may be just a matter of figuring out which graphic programs should save the jpegs. 

17. Integration with Protonmail Bridge. Allows people to send CloudCoins directly from their desktop using protonmail if they have Protonmail bridge installed. 





# Authenticity Number Specification


Example of AN:
dffa2104aa6e4196be3661c06c66b82b

Length:
An AN (Authenticity Number) is 16 byte number show as a 32 character hexadecimal string. It should only contain characters 0-1 and A-F. It can be uppercase or lowercase (case insensitive).  


Example of an AN number that has been separated by hyphens for human readability (ANs are not sent with hyphens)

dffa2104-aa6e4196-be3661c0-6c66b82b

Parts:
The AN is divided into four parts. Each part is 4 bytes long and show with 8 hexadecimal characters. The names are the parts in order are:  Password, PIN, Biometric and Email. 

Default AN:
By default, all parts of the AN are random numbers.

Hashing:
When embedding any information in the AN, such as a Password, PIN, Iris Code, or Email Address, this information must be a hash (using MD5) of the information - not the raw information along with the serial number of the coin. The hash must be done by concatenating the serial number of the CloudCoin with the data. So, suppose that we wanted to embed the pin number "1265" with the CloudCoin serial number "13,465,444".  The string "13465444" would be concatenated with "1265" to become "134654441265".
Then it will be run against a MD5 has to become:

"134654441265" = 1448D3DC1CA9F647AB4841E12AA4FB7D

Spanning:
Spanning is when a number is spanned or distributed among many RAIDA. Spanning stops anyone from gaining information if one of the RAIDA should be compromised. The rules of spanning are that you each character in the hash will be sent to a different RAIDA and then that will be looped until each RAIDA has 4 bytes or 8 characters. So the following code will be 

Here is the distribution of this hash: 1111222233334444555566667777

RAIDA ID Data put in AN
00:17655432
01:17665433
02:17765443
03:11765543
04:21766543
05:21776544
06:21176554
07:22176654
08:32177654
09:32117655
10:32217665
11:33217765
12:43211765
13:43221766
14:43321776
15:44321176
16:54322176
17:54332177
18:54432117
19:55432217
20:65433217
21:65543211
22:65543221
23:66543321
24:76544321




1st: CloudCoin Part.
2nd: PIN
3rd: Biometric
4th: UserEmail or Contact info


CloudCoin Part:
A random number that should always be stored in the CloudCoin file. This number is unique for each RAIDA. 
Unless using "Store in Mind" and then it is a user password that is spanned accross all RAIDA. 

PIN
This is the either a random number or a PIN number that has been hashed with the serial number. 
The hash will have been done on the client side. The PIN is spanned across all RAIDA with the same serial number. 


Biometric
This is either a random number or a hash of a persons' biometric data with the serial number of the CloudCoin. If it is a biometric hash, that has will have been done on the client's side. 
The biometric hash id spanned across all RAIDA. 
