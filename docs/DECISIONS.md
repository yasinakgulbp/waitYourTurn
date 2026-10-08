# Karar kaydı

Tarih: 2026-10-07. `Kararlaştırıldı` kullanıcı tarafından netleştirilen tasarım veya görev kapsamıdır. `Teknik öneri` uygulanmadan önce ilgili aşamanın testinde doğrulanır; kullanıcı onayıymış gibi sunulmaz.

## Kararlaştırılanlar

| Kimlik | Karar | Gerekçe / etkisi |
| --- | --- | --- |
| D01 | Modüler, az bağımlı mimari; küçük ve doğrulanmış aşamalar | Ana mekanikler birbirine uyumlu gelişecek; tek seferde tüm oyun yazılmayacak. |
| D02 | Gameplay tren zemini sabit; çevre görsel olarak hareket eder | Kullanıcı seçti. Hareketli platformun navigasyon/physics yükü ilk kapsamda yok. |
| D03 | İlk istasyon savunması 60 saniye; süre veriden değişir | Kullanıcı seçti. Tüm zombilerin ölmesi beklenmez. |
| D04 | İçeri giren zombiler trende kalır; dışarıdakiler istasyonda kalır | Kullanıcı seçti. Yolculuğun görünen bölümünde tehlike devam eder; içeride/dışarıda üyelik takip edilir. |
| D05 | Can, para, silah ve dron oyuncuyla gider; kapı hasarı/turretler vagonlarında kalır | Kullanıcı seçti. Vagonların durumları ayrı ve koşu boyunca kalıcıdır. |
| D06 | Karanlık geçişte oynanış durur; görünür yolculukta savaş sürer | Kullanıcı seçti. Kontrol/görüş olmayan sürede haksız hasar ve kaynak üretimi yok. |
| D07 | İyileştirme mevcut canın %50'si, minimum 30; maksimum canla sınırlı | Kullanıcı seçti. 10/100 → 40/100, 50/100 → 80/100. |
| D08 | Kapılar hasarla kırılır; vagonlara göre maksimum canları farklıdır | Kullanıcının ana mekaniği. Prototipteki zamanlı kapılar yeni davranışla değiştirilir. |
| D09 | Kırık kapıya yakın kalınca yaklaşık 3 saniyede onarım; ayrılınca iptal | Kullanıcının ana mekaniği. Süre ayarlanabilir; yarım onarım kapıyı tamamlamaz. |
| D10 | Turret ve dron sınırlı mermi kullanır; dron oyuncuyu izler ve mermi bitince kamikaze yapar | Kullanıcının ana mekaniği. Satın alma sınırları ve hedef kaybı yönetilir. |
| D11 | İngilizce, aşamalara ait küçük commitler; repo içi ilerleme kaydı | Kullanıcının çalışma tercihi. Kaynak koddaki mevcut değişiklikler plan commitine alınmaz. |
| D12 | Dış incelemeler öneridir; değişiklikler oyun hedefi ve somut gerekçeyle seçilir | Kullanıcının tercihi. Model uzlaşması veya nezaket tasarım kararının kanıtı değildir. |
| D13 | Sol joystick ile hareket, en yakın görüşü açık zombiye otomatik nişan ve ateş | Kullanıcı M3 için seçti; masaüstünde WASD/ok tuşları aynı hareket girdisine bağlanır. Sağ joystick gerekmez. |
| D14 | İlk tabanca sınırlı şarjör, sınırsız yedek mermi ve otomatik doldurma kullanır | Kullanıcı M3 için seçti. İlk lab değerleri deneme ayarıdır: 8 mermi, 1.2 s doldurma, 0.35 s atış aralığı. |
| D15 | Sağlam kapının üst camından dışarıdaki zombilere ateş edilir; alt dolu panel ve vagon duvarları atışı keser | Kullanıcı M3 değerlendirmesinde netleştirdi. Oyuncu ve ileride kapı önündeki turretler zombileri kapı kırılmadan vurabilir. Kapının yürüyüş engeli ile atış engeli ayrıdır. |
| D16 | Oyuncu ilk kapsamda atandığı vagondan dışarı yürüyemez; kapı kırılması bu izni değiştirmez | Kullanıcı M3 değerlendirmesinde seçti. Hareket alanı ayrı ve isteğe bağlıdır; ileride istasyona inme görevi bu sınırı kaldırabilir/değiştirebilir. Zombi girişini veya atışı engelleyen ek fizik duvarı kurulmaz. |
| D17 | Kayıt hedefi mevcut koşuya devam, ölümde yeni koşu; kalıcı geliştirmeler ayrıca tasarlanır | Kullanıcının ilk seçimi; ardından sağlayıcı/ayrıntılarda emin olmadığını belirtti. Google Play Games Saved Games adaydır, kesin sağlayıcı seçilmedi ve M4'te disk/bulut entegrasyonu yapılmaz. |
| D18 | Ortak temel üzerinde botlu hayatta kalma yarışı (Battle) ve botsuz tek oyunculu ilerleme/hikâye modu hedeflenir | Kullanıcının yeni vizyonu: tren giriş sunumu, nickli komşu vagon rakipleri, son hayatta kalan/sıralama; en az 3 bot beceri profili. Gerçek ağ oyuncuları bu talepten çıkarılmaz. M4a önce ortak döngüyü kanıtlar; mod/bot aşaması ayrı kurulur. |
| D19 | Tabanca sınırsız yedek; hafif makineli, tüfek ve pompalı toplam sınırlı mermi kullanır | Kullanıcı M5'te seçti (2026-10-08). Şarjör bitince yedek varsa otomatik doldurma; son dolum kısmi olabilir. Yeni silah alma/mermi ürünü M7'de tasarlanır; silah veya vagon değiştirmek mermi vermez. |

## Teknik öneriler

| Kimlik | Öneri | Doğrulama aşaması |
| --- | --- | --- |
| T01 | Yakın istasyon/vagon zeminlerinde kesintisiz NavMesh + kapıda carving | İlk link deneyi A54'te doğrulandı; kullanıcı düzenli ikili giriş ve uzak köprü geometrisini uygun bulmadı. 2026-10-07 revizyonu: kapı vagon kenarında, zeminler 0.05 m aralıklı ve kısa eşikle bağlı; sabit geçiş hatları kaldırılır. Gerçek ayrık geometride link seçeneği ayrıca değerlendirilebilir. |
| T02 | Kapı/zombi/oyuncu için ortak hasar ve sağlık sözleşmesi | M2: sağlık sınırları, tek ölüm/ödül, farklı kaynaklar. |
| T03 | İlk silahlar hitscan/pellet; satın alınmış projectile assetleri VFX | M2/M3: tabanca temeli; M5: çeşitlilik. Yavaş projectile gelecekte ayrı resolver kullanır. |
| T04 | Küçük enemy durum makinesi, veri tabanlı tür farklılıkları | M3/M6: türler aynı döngüyü kullanır; gereksiz framework eklenmez. |
| T05 | Spawn canlı sınırları, havuzlar ve dağıtılmış AI/path güncellemeleri | M6/M9: cihazda ölçüm; sayısal bütçeler profil sonucuyla belirlenir. |
| T06 | Sabit savunma slotları; ilk mağazada otomatik para ödülü | M7: daha sade mobil etkileşim. Fiziksel para toplama ileride eklenebilir. |
| T07 | Rastgele atamada eşit vagon olasılığı; aynı vagon tekrar çıkabilir | M4: tasarım denemesi. Kullanıcı henüz bu dağılımı seçmedi. |
| T08 | Ölümde oyun biter; kararma/onarım/satın alma eski koşuyu yeniden başlatamaz | M2/M4: ölüm her evrede geçerli. |
| T09 | M1'de kaba Android kalabalık ölçümü, maliyetlerin ayrılması | Ölçüm olmadan NavMesh değiştirilmez veya mobil performans kanıtlandı denmez. |
| T10 | Minimum mobil kontrol/tabanca M3'e alınır; kullanıcıyla oynanış değerlendirmesi | M3: hareket–ateş–onarım deneyimi; M5: kontrol iyileştirme/silah çeşitliliği. |
| T11 | M4a iki vagonla döngü; M4b beş vagona genişletme | Önce atama/kalıcılık/yolculuk kanıtlanır, sonra geometri ve yük genişler. |
| T12 | Botlu yarış nick/karar/hareketle canlı hissi verir; bilgisayar rakipleri olduğu anlaşılır | Gerçek çevrimiçi insan eşleşmesi diye sunmak güven beklentisini bozabilir. Bu açıklık teknik öneridir; kullanıcı tarafından ayrıca seçilmiş sayılmaz. |

## Uygulama öncesi veya ilgili aşamada netleştirilecekler

Bu liste ilk navigasyon/hasar denemesini engellemez. İlgili özellik başlamadan karar verilir; yanıt alınmamış tercih kesinleşmiş gibi uygulanmaz.

| Konu | İlk öneri / açık detay | Gerektiği aşama |
| --- | --- | --- |
| Hedef Android cihazları | En az bir düşük ve bir orta sınıf gerçek cihaz seçmek; ölçüm olmadan sabit FPS garantisi yok. | M1 ölçüm düzeni, M6 ilk kalabalık bütçesi |
| Vagonlar arası geçiş | D16: oyuncu atandığı vagonda kalır, kırık kapıdan istasyona çıkamaz. Gelecekte görev bazlı inme izni açıkça verilir. Zombilerin başka vagona gidip gitmeyeceği ayrı. | M3/M4 |
| Onarımda kapanış ve saldırı | Geçitte karakter varsa kapanış beklesin; ateş edilebilir. Hasar alınca onarımın iptali henüz seçilmedi. | M3 |
| Doğuş güvenliği | Duvar/karakter içine doğmama + kısa doğuş koruması önerisi; içerideki zombilerin konumu korunur. | M4 |
| Kalkış uyarısı | İlk öneri 60 saniye savunma sonrasında 3 saniye uyarı; fade/yavaşlama süreleri ayrı ayarlar. | M4 |
| Düşman hedef önceliği | Atanmış vagonda oyuncu, turret veya başka hedeflerin önceliği; boş vagondaki zombi davranışı. | M3/M7 |
| Savunmaların canı ve patlaması | Zombiler turret/drona zarar verebilir mi? Turretin mermi bitiş patlaması sadece efekt mi? Dron patlaması duvar/kapıya nasıl etki eder? | M7 |
| Turret sınırının kapsamı | Her türden 2 adet örneği: oyuncu başına mı vagon başına mı? Öneri vagon/slot başına, koşu toplamı ayrıca sınırlı. | M7 |
| Mobil kontrol hissi | Şema D13 ile seçildi; sol joystick ve otomatik ateşin ergonomisi kullanıcı/cihaz denemesinde değerlendirilir. | M3 değerlendirmesi |
| Silah çeşitlerinin mermisi | D19 ile sınırlı yedek seçildi; satın alma, tekrar silah alma ve mermi ürünü ayrıntıları açık. | M7 |
| Mağaza erişimi | Savaşta açık mı, yalnızca yolculukta mı? Öneri gerçek zamanı durdurmayan kolay mobil UI. | M7 |
| Para toplama | Kullanıcı para toplama istedi; ilk öneri ödülü otomatik cüzdana yazmak. Görsel pickup/yerden toplama tercihi açık. | M7 |
| Koşu devamı / metagelişim | Hangi kazanımların kaldığı ve devam beklentisi erken kâğıt üzerinde seçilir; disk kaydı ihtiyaç kadar uygulanır. | M4a öncesi tasarım, M8 uygulama |
| Ödüllü reklam / IAP'nin ekonomi rolü | Verilen ürünler, tekrar sınırları ve denge etkisi erkenden seçilir; entegrasyon daha sonra yapılır. | M7 öncesi tasarım, M9 entegrasyon |

## Bilinen tasarım riskleri

- Kalıcı kapı hasarı ve rastgele vagon ataması, oyuncuyu çok tehlikeli bir vagona getirebilir. Bu istenen risk/şans hissini destekler; anında kaçınılmaz ölüm olmaması için doğuş güvenliği gerekir.
- Turretler eski vagonlarda kaldığından oyuncu dışında öldürme/ödül devam edebilir. Karanlıkta tüm oynanış durur; görünür zamanda bu etki ekonomi dengesiyle değerlendirilir.
- Süreli kalkışta içeride kalan zombi sayısı her istasyonda artabilir. Global canlı sınırı sadece yeni spawnı değil taşınan zombileri de saymalıdır; içeri girmiş zombi sırf bütçe doldu diye yok edilmez.
- Kapıyı onarıp tekrar kırdırmanın ücretsiz onarım döngüsü, güvenli bir sonsuz savunmaya dönüşebilir. Onarım süresi, menzil, kapanış güvenliği ve saldırı temposu birlikte oynanarak dengelenir.

## Karar değişikliği biçimi

Yeni kayıt: tarih, ilgili kimlik, önceki karar, yeni karar, değişiklik nedeni, etkilenen aşamalar/dosyalar ve yeniden yapılacak kontrol. Teknik öneri başarılı deneyden sonra doğrulandı olarak işaretlenir; oyuncu tasarımı ayrıca kullanıcıyla netleştirilir.

## 2026-10-07 — Dış inceleme sonrası plan revizyonu

Kullanıcı mantıklı bulunan önerilerin belgelere işlenmesini istedi. Önceki M3 masaüstünde, mobil kontrol M5'te; M4 beş vagonlu tek aşamaydı. Yeni sıra T09–T11 ile erken cihaz ölçümü, mobil oynanış değerlendirmesi ve iki vagonlu ilk döngüyü içerir. `ROADMAP.md` yeniden kullanım tablosu, kapsam daraltma seçenekleri ve gerçek çalışma hızından sonra süre tahmini yaklaşımıyla güncellendi.

Eklenenler: kontrol/mermi, koşu devamı/metagelişim ve reklam/IAP ekonomi tasarımının erken konuşulması; riskle orantılı otomatik/elle test ayrımı. Korunanlar: NavMesh başlangıç tercihi, ortak hasar/yaşam döngüsü kuralları, pooling eklendiği anda temizlik kontrolü ve dronun oyun kimliğindeki önemi. Waypoint'e erken geçiş, gerekli yaşam döngüsü doğrulamalarını erteleme ve dronu otomatik yayın kapsamından çıkarma alınmadı.

Etkilenen belgeler: ROADMAP, DECISIONS, ARCHITECTURE, GAME_DESIGN ve geliştirme kaydı. Kod değişmedi; yeni kabul koşulları henüz denenmedi ve hiçbir geliştirme aşaması tamamlandı işaretlenmedi.
