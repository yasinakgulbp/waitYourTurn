# A54 cihaz ölçümü — 2026-10-09

Birleşik TrainIntegration, Unity 6000.6.4f1, Android 16/API 36, Samsung SM-A546E (8 GB), ARM64 IL2CPP Development + Profiler, Vulkan/Mali-G68, kalite 2. Çözünürlük 2340×1080 yatay; safe area dışı şeritler siyah. Hedef 60 FPS; optimized frame pacing açık. Telefon USB'den güç alır, pil %100. Adaptif parlaklık açık; başlangıç sistem değeri 64/255, parlaklık sabitlenmedi. Ortam sıcaklığı ölçülmedi.

## Yöntem

Yerel CSV her 10 saniyede gerçek kare aralıklarını sıralayıp FPS, p95/p99, en uzun aralık, 33,34/50 ms üzeri kare sayısı, canlı düşman zirvesi, bellek/GC ve termal durumu kaydeder. P95/p99 **10 saniyelik pencerelere** aittir; pencere yüzdelikleri toplanıp bütün oturum yüzdeliği olarak sunulmaz. Isı ayrıca 30 saniyelik ADB `dumpsys thermalservice` örnekleriyle incelenir; sticky pil bildiriminin sıcaklığı gecikebilir. AP, BAT ve SKIN sensörleri farklı ölçümlerdir.

İsteğe bağlı fixture: Battle insan + dört bot; ölümsüz savunucular, normal kapılar/hasar/atış/AI, ek ama sınırlı spawn (vagon 12, tren 60), normal turret ve dron aktörlerinin yeniden alınması, gerçek istasyon/karanlık/atama döngüsü. İnsan için üç joystick hareketi de uygulandı. Hedef oynanış dengesi değildir; yoğun mekanik yük ve döngü dayanıklılığıdır. Bütün vagonlarda AI çalışır; kadrajda mevcut vagon ve komşuları görülür. Karanlıkta savaş kuralları gereği durur, render/ölçüm sürer; test 10 dakika boyunca sürekli açık ekran/ön plandadır.

Normal koşu önce kaydedilir; fixture ayrı `perf-fixture-run.json` kullanır ve normal koşuya geri dönüşte yüklenir. Tam ham CSV, sensör okumaları, ekranlar ve kayıt incelemesi Builds altında yerel/Git dışıdır; kişisel başka uygulama günlükleri toplanmaz. Yalnız 60 fixture ölçüm satırının kopyası [generated/a54-20261009-combat.csv](generated/a54-20261009-combat.csv) içindedir; cihaz seri numarası, IP, oyuncu kaydı veya runId içermez. Kodun son küçük revizyonunda karanlık evreden fixture kapatırken koruma bayraklarının yeniden sıfırlanması ve FPS hedefi değişiminde eski pencerenin kapanması eklenir; ana 10 dakikalık örnek önceki teşhis sürümündendir, aynı oyun mekaniğini ölçer.

## Kabul ve açık işler

Son küçük revizyonun BuildReport'u Succeeded (38,5 sn); APK yeniden `install -r` ile kuruldu ve 2340×1080 yatay açılış görüldü. Yeni normal savaş örnekleri 59,81–60,01 FPS; bu kısa yeniden kontrol 10 dakikalık fixture'ın tekrar çalıştırılması değildir. Development APK boyutu 43.728.990 bayt; teşhis/JNI/Profiler yükü içerir, yayın APK boyutu kabulü değildir.

Yatay cihaz açılışı görüldü; üç dokunma/sürükleme sonrasında `Move` metni veya eski joystick izi görülmedi, siyah kenar temiz. Pil tüketimi **ölçülmedi**; USB'de yüzde/akım üzerinden tüketim veya pil ömrü çıkarılamaz.

### Tamamlanan 10 dakika

Fixture başlangıç 05:24:47,379 UTC; bitiş 05:34:47,400 UTC. Duraklama/arka plan işareti yok. 60 pencere, 35.986 kare, 600,01 saniye; yedi savunma tamamlanıp sekizinci istasyon yaklaşımına gelindi. Global zirve 60 canlı düşman. Dronun gerçek Following/ammo durumu ve kalabalık ekran görüntüsünde görüldü.

| Ölçüm | Gerçek sonuç |
| --- | --- |
| Toplam kare/aralık toplamından ortalama FPS | 59,97 |
| 10 sn pencere FPS aralığı | 59,41–60,01 |
| 10 sn p95 değerlerinin en yükseği | 16,77 ms |
| 10 sn p99 değerlerinin en yükseği | 33,31 ms |
| En uzun fixture kare aralığı | 50,17 ms |
| 33,34 ms üzeri / 50 ms üzeri | 5 / 1 kare |
| Android termal durum | Bütün örnekler 0 (normal) |
| Pil BAT: test öncesi / son okuma | 29,1 / 30,6°C |
| Yüzey SKIN: test öncesi / zirve / son okuma | 32,2 / 34,3 / 33,8°C |
| AP: test öncesi / zirve / son okuma | 37,1 / 42,2 / 36,5°C |
| Unity allocated: ilk / son fixture penceresi | 101,82 / 102,54 MiB |
| Managed bellek: ilk / son fixture penceresi | 8,98 / 9,10 MiB |
| GC0 sayacı: ilk / son fixture penceresi | 152 / 697 |

Son doğrudan termal okuma fixture bitiminden yaklaşık 35 saniye sonradır; başlangıç okuması başlamadan yaklaşık 35 saniye öncedir. ADB sensör örnekleri 05:26:59–05:34:01 arasında 30 sn aralıklıdır; zirve örneklenmiş zirvedir, sürekli donanım kaydı değildir. CSV'nin sticky BAT değeri tüm oturum 29,6°C kaldığı için pil ısısında doğrudan thermalservice okunması esas alındı. Termal durum 0, Android'in bildirdiği baskı yok anlamına gelir; tüm CPU frekanslarının sabit kaldığını kanıtlamaz.

İlk fixture dakikası ortalama ve son dakika ayrıca karşılaştırılabilir; son pencereler 59,9–60 FPS düzeyindedir, ısıya bağlı kalıcı FPS düşüşü gözlenmedi. PSS başlangıç yakınında 334.656 KiB, 8–9. dakika 299.073 KiB; swap PSS 189→85.851 KiB. İşletim sistemi sayfalama yapmış olabilir; yalnız PSS azalmasına bakarak sızıntı yok veya bütün süreç belleği sabit iddiası yapılmaz. Unity allocated değeri yaklaşık 102 MiB çevresinde kaldı; üretim UI/uzun oturum için GC ve süreç bellek profili yine açıktır.

Otomatik bitiş sonrasında normal dosyadaki **runId öncekiyle aynı**; fixture istasyon 8'den eski koşunun istasyon 1 kaydı yüklendi, ardından normal istasyon 2 savunmasına devam etti; insan canı 100. Test kaydı oyuncunun normal koşusunu değiştirmedi. Bu örnek normal geri dönüşü doğrular; diğer mod/dark-stop varyantlarının tamamının kabulü sayılmaz.

Açılıştaki `ClassNotFoundException: com.google.android.play.core.assetpacks.AssetPackManager` bu APK'da devam eder; süreç/savaş ölçümü sürer, fakat temiz yayın hata kabulü değildir. Uygulama scriptleri AssetPacks API'si çağırmıyor; üretilen Gradle'da PlayCore bağımlılığı yok, asset pack/split binary kullanılmıyor. [Unity'nin resmi API belgesi](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/android/androidassetpacks) bağımlılığı açıklar; mevcut hatanın kesin kök nedeni/fix'i henüz doğrulanmadı. Sırf satırı gizlemek için PlayCore/stub/engine yamaları eklenmedi.

İlk açılışın normal örneği 2076,87 ms uzun aralık içerir; kurulum sonrası başlangıç/ilk sahne yükünü kapsayan ilk kare, 10 dakika fixture sonuçlarından ayrı tutulur. Soğuk başlangıç süresi ayrıca ölçülmelidir. Geliştirici IMGUI HUD her karede string/style/LINQ üretir; ölçümde GC artışı görünür. Üretim HUD'ı uGUI/TMP ile güncellenecek; ölçümler bu yükü de içerir. Son sanat/animasyon/efekt, gerçek iki mod, reklam/SDK, yayın benzeri build, düşük cihaz/tablet ve kablosuz 15–30 dakika testi ayrı kabul kapılarıdır. Tek A54 örneği Google Play bütün kalite kriterleri veya mağazada tanıtım/gelir garantisi sayılmaz.
