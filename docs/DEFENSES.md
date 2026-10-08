# M7b — Sabit turretler

2026-10-08: Normal/gelişmiş turret modülü. Son kullanıcı revizyonu: sabit pad yerine oyuncunun bulunduğu konuma kurulum. Sonraki iş M7c dron; botlar, kayıt ve gelir servisleri bu aşamaya dahil değil.

## Oynanış ve ayarlar

Oyuncu kendi vagonunun herhangi bir uygun noktasında turret satın alır; satın alma anındaki oyuncu X/Z konumuna, vagon zeminine kurulur. Sarı padler ve satın alma yaklaşma mesafesi kaldırıldı. Başka turret/zombi/metal küçük kurulum hacmini işgal ediyorsa veya konum vagon dışındaysa para harcanmaz. Alıcının bedeni uygunluk sorgusunda hariçtir. Vagon başına **her türden en fazla 2**; kapı sayısından bağımsız dört havuz aktörü/vagon, beş vagonda toplam 20 aktör vardır.

| Başlangıç değeri | Normal | Gelişmiş |
| --- | ---: | ---: |
| Fiyat | 90 | 160 |
| Mermi (yedek yok) | 50 | 80 |
| Atış aralığı, sn | 1 | 0,4 |
| Tek atış hasarı | 4 | 4 |
| Menzil, m | 9 | 10 |
| Bitiş patlaması hasarı | 12 | 18 |
| Bitiş patlaması yarıçapı, m | 1,8 | 2 |

Fiyat/başlangıç parası `Content/Economy/RunShop.asset`, silahlar `Content/Defenses/*Weapon.asset`, patlama/renk/hedef yenilemesi `Normal.asset` / `Advanced.asset` içindedir. Denemeler için başlangıç parası geçici **1000**, yayın dengesi değildir. `TurretRack` limit, havuz ve vagon yerel kurulum sınırını yönetir. Mevcut assetleri builder yeniden yazmaz.

Turret görüşü açık en yakın düşmanı seçer, 360 derece döner; arama varsayılan 0,2 sn aralıklıdır. Oyuncuyla aynı `HitscanWeapon`/`HitscanResolver` kullanılır. Camdan geçer, metal çerçeve/panelde durur; bedenler gerçek atışı engelleyebilir. Zombiler şimdilik turretlere saldırmaz. Görsel taban 0,5 m kalırken collider **0,22 × 0,6 × 0,22**, carving **0,24 × 0,7 × 0,24 m** olur. Sahibi turretin içinden yürüyebilir; zombiler küçük engelin etrafından gider. Yalnız fizik çarpışma çifti hariç tutulur, atış kuralı değişmez; kapanırken bu hariç tutma temizlenir.

Son mermi gerçek isabetini tamamladıktan sonra **bir** alan hasarı uygulanır. Yalnız canlı düşmanlar etkilenir. Cam geçirgendir, metal patlama görüşünü keser; düşman bedenleri birbirini korumaz. Merkez mekanik namludur, kozmetik küre gerçek menzili belirlemez. Kısa efekt sonrası aktör kapanır; boş kapasite başka konumda kullanılabilir.

Turret kalan mermisiyle kurulduğu vagonda kalır. İnsan başka vagona geçince görünür zamanda ateş edebilir ve sahibine ödül kazandırır; boş vagona yeni zombi üretimini açmaz. Karanlıkta/ölümde atış kapanır, yeni koşuda kurulumlar temizlenir. Sahip/hedef yaşam kuşağı kontrol edilir. Bot sahipliği/elenmesi Mod/Bot işidir.

## Sorumluluk ve maliyet

`WaitYourTurn.Defenses` yalnız Combat ve Combat.Unity referanslarını kullanır. Controller atış/mermi/bitişi, Rack kota/havuzu, Mount kurulum uygunluğunu, Presentation görseli yönetir. `TurretMount` adı eski pad sürümünden kalır; artık fiziksel pad değil, hareket ettirilen havuz slotudur. `RunDefenses` evre/atama/satın alma adaptörüdür; Economy saf çekirdeği Defenses/Unity bilmez.

Satın almada GameObject/materyal üretilmez; 20 aktör yeniden kullanılır. Managed silah state dizileri ve sahip collider listesi satın almada oluşturulur; sıfır-allocation iddiası yoktur. Her aktör tek çizgi/efekt taşır. Hedefler canlı registry'den okunur, sahne taraması yapılmaz. Yer uygunluğu yalnız satın alma sırasında ölçülür; HUD her çizimde fizik sorgusu yapmaz. Uzak vagon optimizasyonu Mod/Bot/M9'da ayrıca ölçülecek.

RadialDamage en fazla 128 hedef/yaşam snapshot'ı alır; callback sırasında yeniden doğana vurmaz. Aşılırsa hasar uygulanmaz, Overflows artar. Ray buffer'ı 64 dolarsa engelli sayılır. Mevcut canlı bütçesi 60; artırılırsa ilgili ölçüm/kontrol de güncellenir. Kozmetik eventinden hasar üretilmez.

## Yeniden doğrulama

- Build Five Wagons sahne ve dört havuz aktörü/vagon kurulumunu üretir. Install Turrets yalnız modül eksikse kurar; eski kurulum revizyonunu güncellemek için tam builder kullanılır.
- Play F12: gerçek satın alma ve tam oyuncu X/Z konumu, sahibin içinden yürümesi, dışarıda/dolu/kota reddi, 50/80 mermi, gerçek cam atışı/ödül, üç zombi geçişi, en az on atamada konum/mermi/karanlık kapısı, eski vagon ödülü, tek bitiş patlaması, aktör yeniden kullanımı, ölüm/Restart.
- F10 cam/metal/kapı/onarım/istasyon ve F9 mağaza etkilenen regresyon kapılarıdır. Önceki modül kabulü `generated/m7b-pc-acceptance.txt`, bu revizyonun sonucu ayrı kanıt dosyasındadır.
- Test fixture'ı tekrarlanabilir sabit tohum kullanabilir; sonunda normal rastgele koşu ayarını geri getirir. Normal oyunda her Restart yeni tohum ve rastgele ilk vagon kullanır.

Model ölçüleri `MODEL_CONTRACT.md` ve `generated/turret-mount-dimensions.csv` içindedir. Nihai model/efekt/ses aynı fizik/atış köküne giydirilir. Android ergonomisi, uzun oturum ve nihai denge açık.
M7c oyuncu dronu ayrı RunDrone adaptörüyle aynı savunma modülüne eklendi. Turret sahiplik/yerinde kalıcılığı korunur; ayrıntı ve testler [DRONE.md](DRONE.md).
