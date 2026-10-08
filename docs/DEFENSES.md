# M7b — Sabit turretler

2026-10-08: TrainIntegration üzerinde PC teknik kabulü geçti. Sıradaki uygulama M7c dron; botlar, disk kaydı ve reklam/IAP servisleri bu aşamada eklenmedi. Android performansı, dokunmatik ergonomi ve nihai denge açık.

## Oynanış ve ayarlar

Oyuncu sarı kurulum işaretinin yanına gelir, mağazadan normal veya gelişmiş turret alır. En yakın pad seçilir; dolu pad yerine uzaktaki başka pad otomatik seçilmez. Oyuncu/zombi/engel kurulum hacmini işgal ediyorsa satın alma reddedilir; para harcanmaz. Padin üstünde durmak yerine yanında durulur. Varsayılan satın alma mesafesi 1,35 m, vagon başına **her türden en fazla 2** kurulumdur. Her kapının iç yanında bir pad bulunur; beş vagonda toplam 24 yeniden kullanılabilir aktör vardır.

| Başlangıç değeri | Normal | Gelişmiş |
| --- | ---: | ---: |
| Fiyat | 90 | 160 |
| Mermi (yedek yok) | 50 | 80 |
| Atış aralığı, sn | 1 | 0,4 |
| Tek atış hasarı | 4 | 4 |
| Menzil, m | 9 | 10 |
| Bitiş patlaması hasarı | 12 | 18 |
| Bitiş patlaması yarıçapı, m | 1,8 | 2 |

Fiyatlar `Content/Economy/RunShop.asset`; silah verileri `Content/Defenses/*Weapon.asset`; patlama/renk/hedef yenilemesi `Content/Defenses/Normal.asset` ve `Advanced.asset` içindedir. `TurretRack` limit ve yakınlık mesafesini sahiplenir. Bunlar nihai denge değil, Inspector'dan değişen başlangıç değerleridir. Mevcut assetler builder tarafından yeniden yazılmaz.

Turret görüşü açık en yakın düşmanı seçer, 360 derece döner; hedef araması varsayılan 0,2 sn aralıklıdır. Gerçek atış oyuncuyla aynı `HitscanWeapon`/`HitscanResolver` yolunu kullanır. Camdan geçer, metal çerçeve/panelde durur; bedenler gerçek atışı engelleyebilir. Zombiler şimdilik turretlere saldırmaz. Turret gövdesi fiziksel ve NavMesh carving engelidir; namlu üstte, geçit padin yanında açık kalır.

Son mermi gerçek isabetini tamamladıktan sonra **bir** alan hasarı uygulanır. Patlama yalnız canlı düşmanları etkiler; oyuncu/kapıları hasarlamaz. Cam geçirgenliği korunur, metal kapak/duvar patlama görüşünü keser. Düşman bedenleri birbirini patlamadan korumaz. Patlama merkezi mekanik namludur; kozmetik kürenin büyüklüğü hasar menzili değildir. Kısa efekt sonrası aktör kapanır, aynı pad yeniden satın alınabilir.

Turret, kalan mermisiyle kurulduğu vagonda kalır. İnsan başka vagona geçince eski turret görünür oynanışta ateş edebilir ve sahibine ödül kazandırır; boş vagon için yeni zombi üretimi açılmaz. Karanlık geçişte/ölümde atış kapanır, yeni koşuda bütün kurulumlar temizlenir. Sahip ve hedef yaşam kuşağı kontrol edilir. Bot sahipliği/elenme politikası Mod/Bot entegrasyonunda ayrıca doğrulanacaktır.

## Sorumluluk ve maliyet sınırları

`WaitYourTurn.Defenses` yalnız Combat ve Combat.Unity referanslarını kullanır. `TurretController` atış/mermi/bitişi, `TurretRack` vagon kotasını, `TurretMount` fiziksel kurulum uygunluğunu, `TurretPresentation` değiştirilebilir görseli yönetir. `RunDefenses` Run katmanında evre/atama/satın alma adaptörüdür; Economy saf çekirdeği Defenses/Unity bilmez. UI kapalıyken çekirdek çalışır.

Kurulum başına yeni GameObject veya materyal üretilmez; önceden yazılmış 24 aktör açılıp kapanır. Her aktör tek tekrar kullanılan çizgi ve bitiş efekti taşır. Satın alma sırasında küçük managed silah state dizileri yeniden oluşturulur; sıfır-allocation iddiası yoktur. Hedef listesi canlı registry'den okunur; sahne taraması yapılmaz. Bütün vagon turretleri şimdilik simüle/render edilir; uzak vagon sunum azaltma Mod/Bot ve M9 işidir.

Ortak `RadialDamage` en fazla 128 hedef/yaşamın snapshot'ını alır, callback sırasında yeniden doğan hedefe vurmaz. Aşılırsa hasar uygulanmaz, `Overflows` artar. Ray buffer'ı 64 dolarsa güvenli biçimde engelli sayılır. Mevcut canlı bütçesi 60'tır; bu limitler artırılırsa ilgili ölçüm/test de güncellenir. Ateş/patlama kozmetik eventinden hasar üretmez.

## Yeniden doğrulama

- Unity menüsü: `Wait Your Turn > Defenses > Install Turrets In Integration` (Ctrl+Shift+F2), yalnız eksik modülü mevcut TrainIntegration'a ekler; geometri/NavMesh'i yeniden üretmez. Tam Build Five Wagons da modülü kurar.
- EditMode silah testleri Ctrl+Shift+F10: **20 PASS**; beş yeni alan hasarı/hedef seçimi vakası metal/cam, friendly, tek isabet/ödül, eski yaşam ve geçersiz parametreleri kapsar.
- Economy menüsü: **12 PASS**; turret ürünü açık tip indeksi ve mevcut işlemsel satın alma korumalarını kullanır. Ctrl+Shift+F12 TextMeshPro ile çakışırsa listeden Economy seçilir.
- Play **F12**: gerçek satın alma, 50/80 mermi, mesafe/dolu/kota reddi, gerçek cam atışı/ödül, kurulu geometri yanından üç zombi girişi, en az on atamada mermi/konum/karanlık kapısı, eski vagon ödülü, üç mermilik geçici tanımla tek patlama/iki hedef/ödül, aynı aktörün yeni yaşamla yeniden kullanımı, ölüm ve Restart kontrol edilir. Geçici tanım asseti değiştirmez, sonunda normal koşuya dönülür.
- F9 mağaza, F10 mevcut kapı/cam/metal/onarım/istasyon ve F11 spawn/boş vagon/doğuş koruması kontrolleri regresyon kapılarıdır. Sonuç ve UTC zamanları `generated/m7b-pc-acceptance.txt` içindedir.

Model ölçüleri `MODEL_CONTRACT.md` ve `generated/turret-mount-dimensions.csv` içindedir. Nihai model/efekt/ses daha sonra aynı fizik/atış köküne giydirilir.
